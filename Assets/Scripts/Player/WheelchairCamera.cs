using UnityEngine;

namespace WheelchairRacing.Player
{
    /// <summary>
    /// Third-person follow camera for the wheelchair.
    /// Features smooth following, momentum-based delay, and slope-aware height.
    /// Can be used standalone or with Cinemachine.
    /// </summary>
    public class WheelchairCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private WheelchairController wheelchair;

        [Header("Position Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -6f);
        [SerializeField] private float baseHeight = 3f;
        [SerializeField] private float heightSlopeMultiplier = 0f;  // Disabled - no height bobbing on slopes

        [Header("Follow Settings")]
        [SerializeField] private float positionSmoothTime = 0.3f;  // Slower follow for stability
        [SerializeField] private float rotationSmoothSpeed = 3f;   // Slower rotation

        [Header("Momentum Effect")]
        [SerializeField] private float momentumDelay = 0f;         // Disabled - no camera lag
        [SerializeField] private float maxMomentumOffset = 0f;     // Disabled

        [Header("Look Settings")]
        [SerializeField] private float lookAheadDistance = 0f;     // Disabled - camera stays fixed on target
        [SerializeField] private Vector3 lookOffset = new Vector3(0f, 1f, 0f);

        [Header("Collision")]
        [SerializeField] private bool avoidCollision = true;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayers = ~0;

        private Vector3 currentVelocity;
        private Vector3 desiredPosition;
        private float currentHeight;

        private void Awake()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<WheelchairController>();
                if (player != null)
                {
                    target = player.transform;
                    wheelchair = player;
                }
            }

            if (wheelchair == null && target != null)
            {
                wheelchair = target.GetComponent<WheelchairController>();
            }

            currentHeight = baseHeight;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            UpdateDesiredPosition();
            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        private void UpdateDesiredPosition()
        {
            // Calculate base position behind target
            Vector3 targetPosition = target.position;
            Quaternion targetRotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

            // Apply base offset
            desiredPosition = targetPosition + targetRotation * offset;

            // Adjust height based on slope
            if (wheelchair != null)
            {
                float slopeAdjustment = wheelchair.SlopeAngle * heightSlopeMultiplier * Mathf.Deg2Rad;
                currentHeight = Mathf.Lerp(currentHeight, baseHeight + slopeAdjustment, Time.deltaTime * 3f);
            }
            desiredPosition.y = targetPosition.y + currentHeight;

            // Apply momentum offset (camera lags behind during fast movement)
            if (wheelchair != null && wheelchair.CurrentSpeed > 1f)
            {
                Vector3 velocityDirection = wheelchair.Velocity.normalized;
                float momentumAmount = Mathf.Clamp(wheelchair.CurrentSpeed * momentumDelay, 0f, maxMomentumOffset);
                desiredPosition -= velocityDirection * momentumAmount;
            }
        }

        private void UpdateCameraPosition()
        {
            Vector3 finalPosition = desiredPosition;

            // Handle collision avoidance
            if (avoidCollision)
            {
                finalPosition = HandleCollision(target.position + lookOffset, desiredPosition);
            }

            // Smooth position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                finalPosition,
                ref currentVelocity,
                positionSmoothTime
            );
        }

        private void UpdateCameraRotation()
        {
            // Calculate look target (slightly ahead of wheelchair)
            Vector3 lookTarget = target.position + lookOffset;

            if (wheelchair != null && wheelchair.CurrentSpeed > 0.5f)
            {
                lookTarget += wheelchair.Velocity.normalized * lookAheadDistance;
            }

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }

        private Vector3 HandleCollision(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.SphereCast(from, collisionRadius, direction.normalized, out RaycastHit hit, distance, collisionLayers))
            {
                // Move camera closer to avoid collision
                return from + direction.normalized * (hit.distance - collisionRadius);
            }

            return to;
        }

        /// <summary>
        /// Set the camera target at runtime
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            wheelchair = newTarget?.GetComponent<WheelchairController>();
        }

        /// <summary>
        /// Snap camera to position immediately (no smoothing)
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;

            UpdateDesiredPosition();
            transform.position = desiredPosition;

            Vector3 lookTarget = target.position + lookOffset;
            transform.LookAt(lookTarget);

            currentVelocity = Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            if (target == null) return;

            // Draw desired position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(desiredPosition, 0.2f);

            // Draw look target
            Gizmos.color = Color.cyan;
            Vector3 lookTarget = target.position + lookOffset;
            Gizmos.DrawWireSphere(lookTarget, 0.15f);
            Gizmos.DrawLine(transform.position, lookTarget);

            // Draw collision sphere path
            if (avoidCollision)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(target.position + lookOffset, desiredPosition);
            }
        }
    }
}
