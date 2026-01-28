using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Main menu controller with Play button.
    /// Auto-creates UI if not assigned.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private string gameSceneName = "Game";
        [SerializeField] private int gameSceneIndex = 1;
        [SerializeField] private bool useSceneName = false;

        private Canvas canvas;

        private void Awake()
        {
            EnsureEventSystemExists();
            EnsureCanvasExists();
            EnsureMenuUIExists();
            SetupButtons();
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

        private void EnsureMenuUIExists()
        {
            if (playButton == null)
            {
                // Create menu panel
                var menuPanel = new GameObject("MenuPanel");
                menuPanel.transform.SetParent(canvas.transform, false);

                var panelRect = menuPanel.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                // Add dark background
                var panelImage = menuPanel.AddComponent<Image>();
                panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

                // Create title
                var titleGO = new GameObject("Title");
                titleGO.transform.SetParent(menuPanel.transform, false);

                var titleRect = titleGO.AddComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0.7f);
                titleRect.anchorMax = new Vector2(0.5f, 0.7f);
                titleRect.anchoredPosition = Vector2.zero;
                titleRect.sizeDelta = new Vector2(600f, 100f);

                var titleText = titleGO.AddComponent<Text>();
                titleText.text = "STAIRS ARE HARD";
                titleText.fontSize = 72;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = Color.white;
                titleText.fontStyle = FontStyle.Bold;
                titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Create Play button
                var buttonGO = new GameObject("PlayButton");
                buttonGO.transform.SetParent(menuPanel.transform, false);

                var buttonRect = buttonGO.AddComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
                buttonRect.anchoredPosition = Vector2.zero;
                buttonRect.sizeDelta = new Vector2(200f, 60f);

                var buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f);

                playButton = buttonGO.AddComponent<Button>();
                playButton.targetGraphic = buttonImage;

                // Set up button colors for hover/press feedback
                var colors = playButton.colors;
                colors.highlightedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
                colors.pressedColor = new Color(0.1f, 0.5f, 0.1f, 1f);
                playButton.colors = colors;

                // Button text
                var buttonTextGO = new GameObject("Text");
                buttonTextGO.transform.SetParent(buttonGO.transform, false);

                var buttonTextRect = buttonTextGO.AddComponent<RectTransform>();
                buttonTextRect.anchorMin = Vector2.zero;
                buttonTextRect.anchorMax = Vector2.one;
                buttonTextRect.offsetMin = Vector2.zero;
                buttonTextRect.offsetMax = Vector2.zero;

                var buttonText = buttonTextGO.AddComponent<Text>();
                buttonText.text = "PLAY";
                buttonText.fontSize = 36;
                buttonText.alignment = TextAnchor.MiddleCenter;
                buttonText.color = Color.white;
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(PlayGame);
            }
        }

        public void PlayGame()
        {
            if (useSceneName)
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneIndex);
            }
        }
    }
}
