using UnityEngine;

namespace WheelchairRacing.Player
{
    /// <summary>
    /// Main physics-based wheelchair controller.
    /// Uses Rigidbody forces for natural momentum and gravity effects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WheelchairController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float forwardForce = 1200f;
        [SerializeField] private float backwardForce = 700f;
        [SerializeField] private float turnTorque = 80f;
        [SerializeField] private float brakeForce = 50f;
        [SerializeField] private float accelerationTime = 0.5f;  // Time to reach full force

        [Header("Physics Settings")]
        [SerializeField] private float groundDrag = 2f;    // Low drag for responsive movement
        [SerializeField] private float airDrag = 0.5f;      // Very low air resistance
        [SerializeField] private float maxSpeed = 20f;
        [SerializeField] private float slopeGravityMultiplier = 1.5f;
        [SerializeField] private float airControlMultiplier = 0.3f;  // Reduced control when airborne

        [Header("Ground Detection")]
        [SerializeField] private float groundCheckDistance = 1.0f;
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private Transform groundCheckPoint;

        [Header("Stability")]
        [SerializeField] private float uprightForce = 10f;
        [SerializeField] private float maxTiltAngle = 45f;

        [Header("Bounce")]
        [SerializeField] private float bounciness = 0.3f;  // 0 = no bounce, 1 = full bounce

        private Rigidbody rb;
        private WheelchairInput inputHandler;

        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float currentSlopeAngle;
        private bool inputEnabled = true;
        private float accelerationTimer = 0f;

        public bool InputEnabled { get; set; } = true;

        public bool IsGrounded => isGrounded;
        public float CurrentSpeed => rb.linearVelocity.magnitude;
        public float SlopeAngle => currentSlopeAngle;
        public Vector3 Velocity => rb.linearVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inputHandler = GetComponent<WheelchairInput>();

            ConfigureRigidbody();
            ConfigureBounceMaterial();
        }

        private void ConfigureBounceMaterial()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                var bounceMaterial = new PhysicsMaterial("WheelchairBounce")
                {
                    bounciness = this.bounciness,
                    bounceCombine = PhysicsMaterialCombine.Maximum,
                    frictionCombine = PhysicsMaterialCombine.Average,
                    dynamicFriction = 0.4f,
                    staticFriction = 0.4f
                };
                collider.material = bounceMaterial;
            }
        }

        private void ConfigureRigidbody()
        {
            rb.mass = 50f;
            rb.useGravity = true; // Explicitly enable gravity
            rb.linearDamping = groundDrag;
            rb.angularDamping = 8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Prevent flipping by constraining rotation on X and Z
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            CheckGround();
            UpdateDrag();

            if (inputHandler != null)
            {
                float moveInput = inputHandler.MoveInput;
                float turnInput = inputHandler.TurnInput;
                ApplyMovement(moveInput, turnInput, inputHandler.IsBraking);
            }

            ApplySlopeGravity();
            ClampSpeed();
            MaintainUpright();
        }

        private void CheckGround()
        {
            Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;

            // Raycast downward to detect ground
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                isGrounded = true;
                groundNormal = hit.normal;
                currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            }
            else
            {
                isGrounded = false;
                groundNormal = Vector3.up;
                currentSlopeAngle = 0f;
            }
        }

        private void UpdateDrag()
        {
            rb.linearDamping = isGrounded ? groundDrag : airDrag;
        }

        private void ApplyMovement(float moveInput, float turnInput, bool isBraking)
        {
            // Block input during countdown or other non-playing states
            if (!InputEnabled)
            {
                return;
            }

            if (isBraking && isGrounded)
            {
                ApplyBrake();
                return;
            }

            // Control multiplier - reduced when airborne
            float controlMultiplier = isGrounded ? 1f : airControlMultiplier;

            // Forward/backward movement
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                // Ramp up from initial force to max force over time
                accelerationTimer += Time.fixedDeltaTime;
                float rampProgress = Mathf.Clamp01(accelerationTimer / accelerationTime);
                // Start at 30% force, ramp to 100%
                float accelerationFactor = Mathf.Lerp(0.3f, 1f, rampProgress);

                float force = moveInput > 0 ? forwardForce : backwardForce;
                Vector3 moveDirection = transform.forward * moveInput * force * controlMultiplier * accelerationFactor;

                // Project movement onto slope when grounded
                if (isGrounded)
                {
                    moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal);
                }

                rb.AddForce(moveDirection, ForceMode.Force);
            }
            else
            {
                // Reset acceleration when not moving
                accelerationTimer = 0f;
            }

            // Turning
            if (Mathf.Abs(turnInput) > 0.01f)
            {
                // Scale turn torque by speed for better control
                float speedFactor = Mathf.Clamp01(CurrentSpeed / 5f);
                float adjustedTorque = turnTorque * turnInput * (0.3f + speedFactor * 0.7f) * controlMultiplier;
                rb.AddTorque(Vector3.up * adjustedTorque, ForceMode.Force);
            }
        }

        private void ApplyBrake()
        {
            if (CurrentSpeed > 0.1f)
            {
                Vector3 brakeDirection = -rb.linearVelocity.normalized;
                rb.AddForce(brakeDirection * brakeForce, ForceMode.Force);
            }
        }

        private void ApplySlopeGravity()
        {
            if (!isGrounded || currentSlopeAngle < 5f) return;

            // Calculate slope direction (downhill)
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

            // Apply additional gravity force along slope
            float slopeFactor = Mathf.Sin(currentSlopeAngle * Mathf.Deg2Rad);
            Vector3 slopeForce = slopeDirection * Physics.gravity.magnitude * slopeFactor * slopeGravityMultiplier;

            rb.AddForce(slopeForce, ForceMode.Acceleration);
        }

        private void ClampSpeed()
        {
            if (CurrentSpeed > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        private void MaintainUpright()
        {
            // Get current tilt
            float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

            if (tiltAngle > maxTiltAngle)
            {
                // Apply corrective torque to stay upright
                Vector3 correctionAxis = Vector3.Cross(transform.up, Vector3.up);
                rb.AddTorque(correctionAxis * uprightForce, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Apply an external impulse force (used for stair bumps, boosts, etc.)
        /// </summary>
        public void ApplyImpulse(Vector3 force)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }

        /// <summary>
        /// Teleport the wheelchair to a position (used for respawning)
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Stop all movement immediately
        /// </summary>
        public void Stop()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground check ray
            Vector3 origin = groundCheckPoint != null ? groundCheckPoint.position : transform.position;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);

            // Draw ground normal
            if (isGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(origin, groundNormal * 0.5f);
            }
        }
    }
}
