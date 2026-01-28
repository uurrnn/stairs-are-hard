using UnityEngine;
using UnityEngine.SceneManagement;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Manages game state including timer, win/lose conditions, and respawning.
    /// Singleton pattern for easy access from other scripts.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Player Reference")]
        [SerializeField] private Player.WheelchairController player;
        [SerializeField] private Player.WheelchairCamera playerCamera;

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform[] checkpoints;
        private int currentCheckpointIndex = -1;

        [Header("Game Settings")]
        [SerializeField] private bool timerEnabled = true;
        [SerializeField] private float countdownTime = 3f;

        public enum GameState
        {
            Countdown,
            Playing,
            Paused,
            Won,
            Lost
        }

        private GameState currentState = GameState.Countdown;
        private float gameTimer;
        private float countdownTimer;
        private int respawnCount;

        public GameState CurrentState => currentState;
        public float GameTime => gameTimer;
        public float CountdownRemaining => countdownTimer;
        public int RespawnCount => respawnCount;
        public bool IsPlaying => currentState == GameState.Playing;

        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<float> OnTimerUpdated;
        public event System.Action OnRespawn;
        public event System.Action<float> OnLevelCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            FindPlayerReferences();
        }

        private void Start()
        {
            StartCountdown();
        }

        private void FindPlayerReferences()
        {
            if (player == null)
            {
                player = FindFirstObjectByType<Player.WheelchairController>();
            }

            if (playerCamera == null)
            {
                playerCamera = FindFirstObjectByType<Player.WheelchairCamera>();
            }

            // Subscribe to restart input
            if (player != null)
            {
                var input = player.GetComponent<Player.WheelchairInput>();
                if (input != null)
                {
                    input.OnRestartPressed += RestartLevel;
                }
            }
        }

        private void Update()
        {
            switch (currentState)
            {
                case GameState.Countdown:
                    UpdateCountdown();
                    break;
                case GameState.Playing:
                    UpdateTimer();
                    break;
            }
        }

        private void StartCountdown()
        {
            countdownTimer = countdownTime;
            SetGameState(GameState.Countdown);

            // Disable input during countdown (but allow gravity/physics)
            if (player != null)
            {
                player.InputEnabled = false;
            }
        }

        private void UpdateCountdown()
        {
            countdownTimer -= Time.deltaTime;

            if (countdownTimer <= 0f)
            {
                StartPlaying();
            }
        }

        private void StartPlaying()
        {
            gameTimer = 0f;
            SetGameState(GameState.Playing);

            // Enable input when playing starts
            if (player != null)
            {
                player.InputEnabled = true;
            }
        }

        private void UpdateTimer()
        {
            if (!timerEnabled) return;

            gameTimer += Time.deltaTime;
            OnTimerUpdated?.Invoke(gameTimer);
        }

        private void SetGameState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameManager] Game state changed to: {newState}");
            OnGameStateChanged?.Invoke(newState);

            // Handle state-specific logic
            switch (newState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Won:
                case GameState.Lost:
                    // Don't pause, let physics settle
                    break;
            }
        }

        /// <summary>
        /// Called when player reaches the goal
        /// </summary>
        public void CompleteLevel()
        {
            if (currentState != GameState.Playing) return;

            SetGameState(GameState.Won);
            OnLevelCompleted?.Invoke(gameTimer);

            if (player != null)
            {
                player.Stop();
            }

            Debug.Log($"Level completed in {gameTimer:F2} seconds!");
        }

        /// <summary>
        /// Called when player falls out of bounds
        /// </summary>
        public void PlayerFell()
        {
            if (currentState != GameState.Playing) return;

            Respawn();
        }

        /// <summary>
        /// Respawn player at last checkpoint or spawn point
        /// </summary>
        public void Respawn()
        {
            if (player == null) return;

            respawnCount++;

            Transform respawnPoint = GetRespawnPoint();
            player.Teleport(respawnPoint.position, respawnPoint.rotation);

            if (playerCamera != null)
            {
                playerCamera.SnapToTarget();
            }

            OnRespawn?.Invoke();
        }

        private Transform GetRespawnPoint()
        {
            // Use last checkpoint if available
            if (currentCheckpointIndex >= 0 && checkpoints != null && currentCheckpointIndex < checkpoints.Length)
            {
                return checkpoints[currentCheckpointIndex];
            }

            // Fall back to spawn point
            if (spawnPoint != null)
            {
                return spawnPoint;
            }

            // Last resort: player's original position
            return player.transform;
        }

        /// <summary>
        /// Register a checkpoint as reached
        /// </summary>
        public void ReachCheckpoint(int checkpointIndex)
        {
            if (checkpointIndex > currentCheckpointIndex)
            {
                currentCheckpointIndex = checkpointIndex;
                Debug.Log($"Checkpoint {checkpointIndex + 1} reached!");
            }
        }

        /// <summary>
        /// Restart the current level
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Load a specific level by name
        /// </summary>
        public void LoadLevel(string levelName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelName);
        }

        /// <summary>
        /// Load next level in build order
        /// </summary>
        public void LoadNextLevel()
        {
            Time.timeScale = 1f;
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.Log("No more levels!");
            }
        }

        /// <summary>
        /// Pause or unpause the game
        /// </summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
            }
            else if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
            }
        }

        /// <summary>
        /// Format time as MM:SS.mmm
        /// </summary>
        public static string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
