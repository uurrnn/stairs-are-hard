using UnityEngine;
using UnityEngine.InputSystem;

namespace WheelchairRacing.Player
{
    /// <summary>
    /// Handles input for the wheelchair using Unity's new Input System.
    /// Supports keyboard and gamepad input with rebindable controls.
    /// </summary>
    public class WheelchairInput : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction brakeAction;
        private InputAction restartAction;

        private Vector2 moveInputRaw;
        private bool isBraking;

        public float MoveInput => moveInputRaw.y;
        public float TurnInput => moveInputRaw.x;
        public bool IsBraking => isBraking;

        public event System.Action OnRestartPressed;

        private void Awake()
        {
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            if (inputActions != null)
            {
                // Use provided Input Action Asset
                var playerMap = inputActions.FindActionMap("Player");
                if (playerMap != null)
                {
                    moveAction = playerMap.FindAction("Move");
                    brakeAction = playerMap.FindAction("Brake");
                    restartAction = playerMap.FindAction("Restart");
                }
            }

            // Create default actions if not provided
            if (moveAction == null)
            {
                CreateDefaultActions();
            }

            BindActions();
        }

        private void CreateDefaultActions()
        {
            // Movement: WASD / Left Stick
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");

            // Brake: Space / A Button
            brakeAction = new InputAction("Brake", InputActionType.Button);
            brakeAction.AddBinding("<Keyboard>/space");
            brakeAction.AddBinding("<Gamepad>/buttonSouth");

            // Restart: R / Start Button
            restartAction = new InputAction("Restart", InputActionType.Button);
            restartAction.AddBinding("<Keyboard>/r");
            restartAction.AddBinding("<Gamepad>/start");
        }

        private void BindActions()
        {
            if (moveAction != null)
            {
                moveAction.performed += OnMove;
                moveAction.canceled += OnMove;
            }

            if (brakeAction != null)
            {
                brakeAction.performed += OnBrake;
                brakeAction.canceled += OnBrake;
            }

            if (restartAction != null)
            {
                restartAction.performed += OnRestart;
            }
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            brakeAction?.Enable();
            restartAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            brakeAction?.Disable();
            restartAction?.Disable();
        }

        private void OnDestroy()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMove;
                moveAction.canceled -= OnMove;
            }

            if (brakeAction != null)
            {
                brakeAction.performed -= OnBrake;
                brakeAction.canceled -= OnBrake;
            }

            if (restartAction != null)
            {
                restartAction.performed -= OnRestart;
            }

            // Dispose actions if we created them
            if (inputActions == null)
            {
                moveAction?.Dispose();
                brakeAction?.Dispose();
                restartAction?.Dispose();
            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInputRaw = context.ReadValue<Vector2>();
        }

        private void OnBrake(InputAction.CallbackContext context)
        {
            isBraking = context.performed;
        }

        private void OnRestart(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnRestartPressed?.Invoke();
            }
        }

        /// <summary>
        /// Get the current raw input vector (for UI display or debugging)
        /// </summary>
        public Vector2 GetRawInput()
        {
            return moveInputRaw;
        }
    }
}
