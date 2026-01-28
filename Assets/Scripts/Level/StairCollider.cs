using UnityEngine;

namespace WheelchairRacing.Level
{
    /// <summary>
    /// Handles stair-specific physics interactions.
    /// Uses invisible ramp collider overlay with optional bump effects.
    /// </summary>
    public class StairCollider : MonoBehaviour
    {
        [Header("Stair Settings")]
        [SerializeField] private StairDirection direction = StairDirection.Down;
        [SerializeField] private int stepCount = 10;
        [SerializeField] private float stepHeight = 0.2f;
        [SerializeField] private float stepDepth = 0.3f;

        [Header("Physics Modifiers")]
        [SerializeField] private float speedMultiplierDown = 1.1f;
        [SerializeField] private float speedMultiplierUp = 0.8f;
        [SerializeField] private float minSpeedToClimb = 5f;

        [Header("Bump Effect")]
        [SerializeField] private bool enableBumpEffect = true;
        [SerializeField] private float bumpFrequency = 8f;
        [SerializeField] private float bumpStrength = 0.5f;
        [SerializeField] private AudioSource bumpSound;

        public enum StairDirection
        {
            Down,
            Up
        }

        private float lastBumpTime;
        private Collider rampCollider;

        private void Awake()
        {
            // Find or create the ramp collider (child object with smooth collision)
            rampCollider = GetComponentInChildren<Collider>();
        }

        private void OnTriggerStay(Collider other)
        {
            var wheelchair = other.GetComponent<Player.WheelchairController>();
            if (wheelchair == null) return;

            HandleStairPhysics(wheelchair);
        }

        private void HandleStairPhysics(Player.WheelchairController wheelchair)
        {
            float currentSpeed = wheelchair.CurrentSpeed;
            Vector3 velocity = wheelchair.Velocity;

            // Determine if going up or down based on velocity and stair direction
            bool goingDown = Vector3.Dot(velocity, GetDownhillDirection()) > 0;

            if (direction == StairDirection.Up && !goingDown)
            {
                // Trying to climb up
                if (currentSpeed < minSpeedToClimb)
                {
                    // Too slow to climb - apply resistance
                    wheelchair.ApplyImpulse(-velocity.normalized * 2f);
                }
            }

            // Apply bump effect for tactile feedback
            if (enableBumpEffect && currentSpeed > 1f)
            {
                ApplyBumpEffect(wheelchair);
            }
        }

        private void ApplyBumpEffect(Player.WheelchairController wheelchair)
        {
            float timeSinceLastBump = Time.time - lastBumpTime;
            float bumpInterval = 1f / bumpFrequency;

            if (timeSinceLastBump >= bumpInterval)
            {
                lastBumpTime = Time.time;

                // Small upward impulse for bump feel
                Vector3 bumpForce = Vector3.up * bumpStrength;
                wheelchair.ApplyImpulse(bumpForce);

                // Play bump sound
                if (bumpSound != null && !bumpSound.isPlaying)
                {
                    bumpSound.pitch = Random.Range(0.9f, 1.1f);
                    bumpSound.Play();
                }
            }
        }

        private Vector3 GetDownhillDirection()
        {
            // Calculate downhill direction based on stair orientation
            return -transform.up;
        }

        /// <summary>
        /// Generate ramp collider to overlay stairs (call from editor script)
        /// </summary>
        public void GenerateRampCollider()
        {
            // Calculate ramp dimensions
            float totalHeight = stepCount * stepHeight;
            float totalDepth = stepCount * stepDepth;

            // Create or update box collider for ramp
            var box = GetComponent<BoxCollider>();
            if (box == null)
            {
                box = gameObject.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            box.size = new Vector3(2f, totalHeight, totalDepth);
            box.center = new Vector3(0f, totalHeight / 2f, totalDepth / 2f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw stair outline
            Gizmos.color = Color.yellow;

            Vector3 startPos = transform.position;
            for (int i = 0; i < stepCount; i++)
            {
                Vector3 stepStart = startPos + transform.forward * (i * stepDepth) + transform.up * (i * stepHeight);
                Vector3 stepEnd = stepStart + transform.forward * stepDepth;
                Vector3 stepTop = stepStart + transform.up * stepHeight;

                Gizmos.DrawLine(stepStart, stepEnd);
                Gizmos.DrawLine(stepEnd, stepEnd + transform.up * stepHeight);
            }

            // Draw climb threshold indicator
            if (direction == StairDirection.Up)
            {
                Gizmos.color = Color.red;
                Vector3 center = transform.position + Vector3.up * 0.5f;
                Gizmos.DrawWireSphere(center, 0.3f);
            }
        }
    }
}
