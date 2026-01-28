using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Manages the in-game UI including timer, countdown, and completion screen.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Timer Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private string timerFormat = "Time: {0}";

        [Header("Countdown Display")]
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject countdownPanel;
        [SerializeField] private Image[] countdownCircles = new Image[3];
        [SerializeField] private Color circleReadyColor = Color.green;
        [SerializeField] private Color circleWaitColor = Color.red;

        [Header("Speed Display")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private string speedFormat = "{0:F1} m/s";
        [SerializeField] private Player.WheelchairController player;

        [Header("Completion Screen")]
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private TextMeshProUGUI completionTimeText;
        [SerializeField] private TextMeshProUGUI respawnCountText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button nextLevelButton;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private int mainMenuSceneIndex = 0;

        private Canvas canvas;

        private void Awake()
        {
            EnsureEventSystemExists();
            EnsureCanvasExists();
            EnsureCountdownUIExists();
            EnsurePauseUIExists();
            SetupButtons();
            HideAllPanels();
        }

        private void EnsureEventSystemExists()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
            }
        }

        private void EnsureCanvasExists()
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
        }

        private void EnsureCountdownUIExists()
        {
            if (countdownPanel == null)
            {
                // Create countdown panel
                countdownPanel = new GameObject("CountdownPanel");
                countdownPanel.transform.SetParent(canvas.transform, false);

                var panelRect = countdownPanel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.anchoredPosition = new Vector2(0f, 50f);
                panelRect.sizeDelta = new Vector2(300f, 100f);

                // Create horizontal layout for circles
                var circleContainer = new GameObject("CircleContainer");
                circleContainer.transform.SetParent(countdownPanel.transform, false);

                var containerRect = circleContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
                containerRect.sizeDelta = new Vector2(200f, 50f);

                var layout = circleContainer.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 20f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;

                // Create 3 circles
                countdownCircles = new Image[3];
                for (int i = 0; i < 3; i++)
                {
                    var circleGO = new GameObject($"Circle{i + 1}");
                    circleGO.transform.SetParent(circleContainer.transform, false);

                    var circleRect = circleGO.AddComponent<RectTransform>();
                    circleRect.sizeDelta = new Vector2(50f, 50f);

                    var circleImage = circleGO.AddComponent<Image>();
                    circleImage.color = circleWaitColor;

                    countdownCircles[i] = circleImage;
                }

                // Create GO! text below circles
                var textGO = new GameObject("CountdownText");
                textGO.transform.SetParent(countdownPanel.transform, false);

                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(0f, -50f);
                textRect.sizeDelta = new Vector2(200f, 50f);

                countdownText = textGO.AddComponent<TextMeshProUGUI>();
                countdownText.text = "";
                countdownText.fontSize = 48;
                countdownText.alignment = TextAlignmentOptions.Center;
                countdownText.color = Color.white;
            }
        }

        private void EnsurePauseUIExists()
        {
            if (pausePanel == null)
            {
                // Create pause panel with dark overlay
                pausePanel = new GameObject("PausePanel");
                pausePanel.transform.SetParent(canvas.transform, false);

                var panelRect = pausePanel.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                var panelImage = pausePanel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.8f);

                // Create title
                var titleGO = new GameObject("PauseTitle");
                titleGO.transform.SetParent(pausePanel.transform, false);

                var titleRect = titleGO.AddComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.7f);
                titleRect.anchorMax = new Vector2(0.5f, 0.7f);
                titleRect.anchoredPosition = Vector2.zero;
                titleRect.sizeDelta = new Vector2(300f, 60f);

                var titleText = titleGO.AddComponent<Text>();
                titleText.text = "PAUSED";
                titleText.fontSize = 48;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = Color.white;
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Create buttons container
                var buttonsContainer = new GameObject("ButtonsContainer");
                buttonsContainer.transform.SetParent(pausePanel.transform, false);

                var containerRect = buttonsContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
                containerRect.sizeDelta = new Vector2(200f, 250f);

                var layout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 15f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = true;
                layout.childControlHeight = false;

                // Create Resume button
                resumeButton = CreatePauseButton("ResumeButton", "RESUME", buttonsContainer.transform);

                // Create Restart button
                pauseRestartButton = CreatePauseButton("RestartButton", "RESTART", buttonsContainer.transform);

                // Create Main Menu button
                mainMenuButton = CreatePauseButton("MainMenuButton", "MAIN MENU", buttonsContainer.transform);

                // Create Quit button
                quitButton = CreatePauseButton("QuitButton", "QUIT", buttonsContainer.transform);
            }
        }

        private Button CreatePauseButton(string name, string text, Transform parent)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(200f, 50f);

            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Set up button colors for hover/press feedback
            var colors = button.colors;
            colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            button.colors = colors;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Use Unity's built-in Text instead of TMP (has default Arial font)
            var buttonText = textGO.AddComponent<Text>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return button;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                GameManager.Instance.OnTimerUpdated += UpdateTimer;
                GameManager.Instance.OnLevelCompleted += ShowCompletionScreen;

                // Show countdown panel if we're already in countdown state
                if (GameManager.Instance.CurrentState == GameManager.GameState.Countdown)
                {
                    if (countdownPanel != null) countdownPanel.SetActive(true);
                }
            }

            if (player == null)
            {
                player = FindFirstObjectByType<Player.WheelchairController>();
            }
        }

        private void Update()
        {
            UpdateCountdown();
            UpdateSpeedDisplay();
            HandlePauseInput();
        }

        private void HandlePauseInput()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null)
                {
                    var state = GameManager.Instance.CurrentState;
                    if (state == GameManager.GameState.Playing || state == GameManager.GameState.Paused)
                    {
                        GameManager.Instance.TogglePause();
                    }
                }
            }
        }

        private void SetupButtons()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(() => GameManager.Instance?.RestartLevel());
            }

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(() => GameManager.Instance?.LoadNextLevel());
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(() => GameManager.Instance?.TogglePause());
            }

            if (pauseRestartButton != null)
            {
                pauseRestartButton.onClick.AddListener(() => GameManager.Instance?.RestartLevel());
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(LoadMainMenu);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() => Application.Quit());
            }
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneIndex);
        }

        private void HideAllPanels()
        {
            if (completionPanel != null) completionPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }

        private void HandleGameStateChanged(GameManager.GameState state)
        {
            switch (state)
            {
                case GameManager.GameState.Countdown:
                    if (countdownPanel != null) countdownPanel.SetActive(true);
                    if (completionPanel != null) completionPanel.SetActive(false);
                    if (pausePanel != null) pausePanel.SetActive(false);
                    break;

                case GameManager.GameState.Playing:
                    // Keep countdown panel visible briefly to show green lights and GO!
                    StartCoroutine(HideCountdownAfterDelay(0.5f));
                    if (pausePanel != null) pausePanel.SetActive(false);
                    break;

                case GameManager.GameState.Paused:
                    if (pausePanel != null) pausePanel.SetActive(true);
                    break;

                case GameManager.GameState.Won:
                    // Completion screen shown via OnLevelCompleted
                    break;
            }
        }

        private void UpdateCountdown()
        {
            if (GameManager.Instance == null) return;

            var state = GameManager.Instance.CurrentState;

            if (state == GameManager.GameState.Countdown)
            {
                float remaining = GameManager.Instance.CountdownRemaining;

                // 3 second countdown: circle 1 at 2s remaining, circle 2 at 1s remaining, circle 3 at 0s
                Color c1 = remaining <= 2f ? circleReadyColor : circleWaitColor;
                Color c2 = remaining <= 1f ? circleReadyColor : circleWaitColor;
                Color c3 = circleWaitColor; // Third circle turns green when GO

                SetCircleColors(c1, c2, c3);

                if (countdownText != null)
                {
                    countdownText.text = "";
                }
            }
            else if (state == GameManager.GameState.Playing)
            {
                // All circles green when playing
                SetCircleColors(circleReadyColor, circleReadyColor, circleReadyColor);

                if (countdownText != null)
                {
                    countdownText.text = "GO!";
                }
            }
        }

        private void SetCircleColors(Color c1, Color c2, Color c3)
        {
            if (countdownCircles == null || countdownCircles.Length < 3) return;

            if (countdownCircles[0] != null) countdownCircles[0].color = c1;
            if (countdownCircles[1] != null) countdownCircles[1].color = c2;
            if (countdownCircles[2] != null) countdownCircles[2].color = c3;
        }

        private IEnumerator HideCountdownAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }

        private void UpdateTimer(float time)
        {
            if (timerText != null)
            {
                timerText.text = string.Format(timerFormat, GameManager.FormatTime(time));
            }
        }

        private void UpdateSpeedDisplay()
        {
            if (speedText == null || player == null) return;

            speedText.text = string.Format(speedFormat, player.CurrentSpeed);
        }

        private void ShowCompletionScreen(float finalTime)
        {
            if (completionPanel == null) return;

            completionPanel.SetActive(true);

            if (completionTimeText != null)
            {
                completionTimeText.text = $"Time: {GameManager.FormatTime(finalTime)}";
            }

            if (respawnCountText != null && GameManager.Instance != null)
            {
                respawnCountText.text = $"Respawns: {GameManager.Instance.RespawnCount}";
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
                GameManager.Instance.OnTimerUpdated -= UpdateTimer;
                GameManager.Instance.OnLevelCompleted -= ShowCompletionScreen;
            }
        }
    }
}
