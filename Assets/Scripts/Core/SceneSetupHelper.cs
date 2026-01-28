using UnityEngine;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Editor helper script for quickly setting up a scene with all required game objects.
    /// Add this to an empty GameObject and use the context menu to create the game structure.
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [Header("Prefab References (Optional)")]
        [SerializeField] private GameObject wheelchairPrefab;

        [Header("Generated References")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject cameraObject;
        [SerializeField] private GameObject gameManagerObject;
        [SerializeField] private GameObject levelManagerObject;
        [SerializeField] private GameObject uiCanvas;

        [ContextMenu("Setup Complete Scene")]
        public void SetupCompleteScene()
        {
            CreatePlayer();
            CreateCamera();
            CreateManagers();
            CreateTestLevel();
            CreateUI();

            Debug.Log("Scene setup complete! Don't forget to tag the player as 'Player'.");
        }

        [ContextMenu("Create Player")]
        public void CreatePlayer()
        {
            if (playerObject != null)
            {
                Debug.Log("Player already exists.");
                return;
            }

            // Create wheelchair object
            playerObject = new GameObject("Wheelchair");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.up * 0.5f;

            // Add capsule collider (simple collision shape)
            var collider = playerObject.AddComponent<CapsuleCollider>();
            collider.height = 1.2f;
            collider.radius = 0.4f;
            collider.center = new Vector3(0f, 0.6f, 0f);

            // Add rigidbody
            var rb = playerObject.AddComponent<Rigidbody>();
            rb.mass = 70f;
            rb.linearDamping = 2f;
            rb.angularDamping = 8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // Add scripts
            playerObject.AddComponent<Player.WheelchairController>();
            playerObject.AddComponent<Player.WheelchairInput>();

            // Add ground check point
            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(playerObject.transform);
            groundCheck.transform.localPosition = new Vector3(0f, 0.1f, 0f);

            // Add wheelchair visual (procedural mesh from primitives)
            playerObject.AddComponent<Player.WheelchairVisual>();

            Debug.Log("Player created successfully.");
        }

        [ContextMenu("Create Camera")]
        public void CreateCamera()
        {
            if (cameraObject != null)
            {
                Debug.Log("Camera already exists.");
                return;
            }

            // Find or create main camera
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraObject = mainCam.gameObject;
            }
            else
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            cameraObject.transform.position = new Vector3(0f, 3f, -6f);

            // Add camera controller
            if (cameraObject.GetComponent<Player.WheelchairCamera>() == null)
            {
                cameraObject.AddComponent<Player.WheelchairCamera>();
            }

            Debug.Log("Camera setup complete.");
        }

        [ContextMenu("Create Managers")]
        public void CreateManagers()
        {
            // Game Manager
            if (gameManagerObject == null)
            {
                gameManagerObject = new GameObject("GameManager");
                gameManagerObject.AddComponent<GameManager>();
            }

            // Level Manager
            if (levelManagerObject == null)
            {
                levelManagerObject = new GameObject("LevelManager");
                levelManagerObject.AddComponent<LevelManager>();
            }

            Debug.Log("Managers created.");
        }

        [ContextMenu("Create Test Level")]
        public void CreateTestLevel()
        {
            var levelParent = new GameObject("Level");

            // Ground plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(levelParent.transform);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 10f);

            // Ramp down
            var rampDown = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampDown.name = "RampDown";
            rampDown.transform.SetParent(levelParent.transform);
            rampDown.transform.position = new Vector3(0f, -1f, 20f);
            rampDown.transform.localScale = new Vector3(5f, 0.2f, 10f);
            rampDown.transform.rotation = Quaternion.Euler(-15f, 0f, 0f);

            // Flat section
            var flatSection = GameObject.CreatePrimitive(PrimitiveType.Plane);
            flatSection.name = "FlatSection";
            flatSection.transform.SetParent(levelParent.transform);
            flatSection.transform.position = new Vector3(0f, -3f, 35f);
            flatSection.transform.localScale = new Vector3(3f, 1f, 5f);

            // Ramp up
            var rampUp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rampUp.name = "RampUp";
            rampUp.transform.SetParent(levelParent.transform);
            rampUp.transform.position = new Vector3(0f, -1.5f, 50f);
            rampUp.transform.localScale = new Vector3(5f, 0.2f, 10f);
            rampUp.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            // Goal platform
            var goalPlatform = GameObject.CreatePrimitive(PrimitiveType.Plane);
            goalPlatform.name = "GoalPlatform";
            goalPlatform.transform.SetParent(levelParent.transform);
            goalPlatform.transform.position = new Vector3(0f, 0f, 65f);
            goalPlatform.transform.localScale = new Vector3(2f, 1f, 2f);

            // Goal zone
            var goalZone = new GameObject("GoalZone");
            goalZone.transform.SetParent(levelParent.transform);
            goalZone.transform.position = new Vector3(0f, 1f, 65f);
            var goalCollider = goalZone.AddComponent<BoxCollider>();
            goalCollider.size = new Vector3(10f, 3f, 5f);
            goalCollider.isTrigger = true;
            goalZone.AddComponent<Level.GoalZone>();

            // Fall zone (large trigger below level)
            var fallZone = new GameObject("FallZone");
            fallZone.transform.SetParent(levelParent.transform);
            fallZone.transform.position = new Vector3(0f, -20f, 30f);
            var fallCollider = fallZone.AddComponent<BoxCollider>();
            fallCollider.size = new Vector3(200f, 5f, 200f);
            fallCollider.isTrigger = true;
            fallZone.AddComponent<Level.FallZone>();

            // Spawn point
            var spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(levelParent.transform);
            spawnPoint.transform.position = new Vector3(0f, 0.5f, 0f);

            Debug.Log("Test level created.");
        }

        [ContextMenu("Create UI")]
        public void CreateUI()
        {
            if (uiCanvas != null)
            {
                Debug.Log("UI already exists.");
                return;
            }

            // Create canvas
            uiCanvas = new GameObject("GameCanvas");
            var canvas = uiCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            uiCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Add GameUI component
            uiCanvas.AddComponent<GameUI>();

            // Create timer text (placeholder - needs TextMeshPro)
            var timerObj = new GameObject("TimerText");
            timerObj.transform.SetParent(uiCanvas.transform);
            var timerRect = timerObj.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -20f);
            timerRect.sizeDelta = new Vector2(200f, 50f);

            Debug.Log("UI canvas created. Add TextMeshPro components for text display.");
        }

        [ContextMenu("Create Stairs Prefab")]
        public void CreateStairsPrefab()
        {
            var stairsParent = new GameObject("Stairs");
            stairsParent.AddComponent<Level.StairCollider>();

            // Visual stairs (10 steps)
            int stepCount = 10;
            float stepHeight = 0.2f;
            float stepDepth = 0.3f;
            float stepWidth = 3f;

            for (int i = 0; i < stepCount; i++)
            {
                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Step_{i}";
                step.transform.SetParent(stairsParent.transform);
                step.transform.localPosition = new Vector3(0f, stepHeight * (i + 0.5f), stepDepth * i);
                step.transform.localScale = new Vector3(stepWidth, stepHeight, stepDepth);
            }

            // Add ramp collider overlay (invisible)
            var rampCollider = new GameObject("RampCollider");
            rampCollider.transform.SetParent(stairsParent.transform);
            var box = rampCollider.AddComponent<BoxCollider>();
            box.isTrigger = true;

            float totalHeight = stepCount * stepHeight;
            float totalDepth = stepCount * stepDepth;
            float angle = Mathf.Atan2(totalHeight, totalDepth) * Mathf.Rad2Deg;

            rampCollider.transform.localPosition = new Vector3(0f, totalHeight / 2f, totalDepth / 2f);
            rampCollider.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            box.size = new Vector3(stepWidth, 0.1f, Mathf.Sqrt(totalHeight * totalHeight + totalDepth * totalDepth));

            Debug.Log("Stairs created. Position and rotate as needed.");
        }
    }
}
