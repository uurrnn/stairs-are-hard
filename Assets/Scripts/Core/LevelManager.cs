using UnityEngine;
using System.Collections.Generic;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Manages level-specific functionality including checkpoints, level loading, and level data.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Info")]
        [SerializeField] private string levelName = "Level 01";
        [SerializeField] private int levelIndex = 1;

        [Header("Checkpoints")]
        [SerializeField] private Transform[] checkpoints;
        [SerializeField] private bool showCheckpointGizmos = true;

        [Header("Spawn")]
        [SerializeField] private Transform playerSpawnPoint;

        [Header("Level Bounds")]
        [SerializeField] private Bounds levelBounds = new Bounds(Vector3.zero, new Vector3(100f, 50f, 100f));

        private HashSet<int> activatedCheckpoints = new HashSet<int>();
        private int lastCheckpointIndex = -1;

        public string LevelName => levelName;
        public int LevelIndex => levelIndex;
        public Transform SpawnPoint => playerSpawnPoint;
        public int LastCheckpoint => lastCheckpointIndex;
        public int TotalCheckpoints => checkpoints?.Length ?? 0;

        public event System.Action<int> OnCheckpointReached;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ValidateSetup();
        }

        private void ValidateSetup()
        {
            if (playerSpawnPoint == null)
            {
                Debug.LogWarning("LevelManager: No spawn point assigned. Using transform position.");
                playerSpawnPoint = transform;
            }

            if (checkpoints == null || checkpoints.Length == 0)
            {
                Debug.Log("LevelManager: No checkpoints defined for this level.");
            }
        }

        /// <summary>
        /// Called when player reaches a checkpoint
        /// </summary>
        public void ActivateCheckpoint(int checkpointIndex)
        {
            if (checkpointIndex < 0 || checkpointIndex >= checkpoints.Length) return;
            if (activatedCheckpoints.Contains(checkpointIndex)) return;

            activatedCheckpoints.Add(checkpointIndex);

            if (checkpointIndex > lastCheckpointIndex)
            {
                lastCheckpointIndex = checkpointIndex;

                // Notify GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ReachCheckpoint(checkpointIndex);
                }

                OnCheckpointReached?.Invoke(checkpointIndex);
            }
        }

        /// <summary>
        /// Get the position for respawning
        /// </summary>
        public Transform GetRespawnPoint()
        {
            if (lastCheckpointIndex >= 0 && lastCheckpointIndex < checkpoints.Length)
            {
                return checkpoints[lastCheckpointIndex];
            }
            return playerSpawnPoint;
        }

        /// <summary>
        /// Check if a position is within level bounds
        /// </summary>
        public bool IsInBounds(Vector3 position)
        {
            return levelBounds.Contains(position);
        }

        /// <summary>
        /// Get checkpoint progress as percentage
        /// </summary>
        public float GetCheckpointProgress()
        {
            if (checkpoints == null || checkpoints.Length == 0) return 0f;
            return (float)(lastCheckpointIndex + 1) / checkpoints.Length;
        }

        /// <summary>
        /// Reset all checkpoints (for level restart)
        /// </summary>
        public void ResetCheckpoints()
        {
            activatedCheckpoints.Clear();
            lastCheckpointIndex = -1;
        }

        /// <summary>
        /// Get all checkpoint transforms
        /// </summary>
        public Transform[] GetCheckpoints()
        {
            return checkpoints;
        }

        private void OnDrawGizmos()
        {
            // Draw level bounds
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawCube(levelBounds.center, levelBounds.size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);

            // Draw spawn point
            if (playerSpawnPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(playerSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(playerSpawnPoint.position, playerSpawnPoint.position + playerSpawnPoint.forward * 2f);
            }

            // Draw checkpoints
            if (showCheckpointGizmos && checkpoints != null)
            {
                for (int i = 0; i < checkpoints.Length; i++)
                {
                    if (checkpoints[i] == null) continue;

                    bool activated = activatedCheckpoints.Contains(i);
                    Gizmos.color = activated ? Color.green : Color.cyan;

                    Gizmos.DrawWireSphere(checkpoints[i].position, 1f);

                    // Draw checkpoint number
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(checkpoints[i].position + Vector3.up * 1.5f, $"CP {i + 1}");
                    #endif

                    // Draw line to next checkpoint
                    if (i < checkpoints.Length - 1 && checkpoints[i + 1] != null)
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
                    }
                }
            }
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
