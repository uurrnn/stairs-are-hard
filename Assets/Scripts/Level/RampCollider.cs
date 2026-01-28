using UnityEngine;

namespace WheelchairRacing.Level
{
    /// <summary>
    /// Handles ramp-specific physics modifiers.
    /// Ramps naturally affect speed through Unity physics, but this allows additional control.
    /// </summary>
    public class RampCollider : MonoBehaviour
    {
        [Header("Ramp Settings")]
        [SerializeField] private RampType rampType = RampType.Standard;
        [SerializeField] private float angle = 15f;

        [Header("Physics Modifiers")]
        [SerializeField] private float accelerationBoost = 0f;
        [SerializeField] private float frictionMultiplier = 1f;

        [Header("Special Effects")]
        [SerializeField] private bool isBoostRamp = false;
        [SerializeField] private float boostForce = 20f;
        [SerializeField] private ParticleSystem boostParticles;
        [SerializeField] private AudioSource boostSound;

        public enum RampType
        {
            Standard,
            Boost,
            Slippery,
            Sticky
        }

        private void OnTriggerEnter(Collider other)
        {
            var wheelchair = other.GetComponent<Player.WheelchairController>();
            if (wheelchair == null) return;

            if (isBoostRamp || rampType == RampType.Boost)
            {
                ApplyBoost(wheelchair);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var wheelchair = other.GetComponent<Player.WheelchairController>();
            if (wheelchair == null) return;

            ApplyRampEffects(wheelchair);
        }

        private void ApplyRampEffects(Player.WheelchairController wheelchair)
        {
            switch (rampType)
            {
                case RampType.Slippery:
                    // Reduced friction - wheelchair slides more
                    ApplyAcceleration(wheelchair, accelerationBoost * 1.5f);
                    break;

                case RampType.Sticky:
                    // Increased friction - wheelchair slows down
                    ApplyDeceleration(wheelchair, accelerationBoost);
                    break;

                case RampType.Standard:
                default:
                    if (accelerationBoost != 0f)
                    {
                        ApplyAcceleration(wheelchair, accelerationBoost);
                    }
                    break;
            }
        }

        private void ApplyAcceleration(Player.WheelchairController wheelchair, float amount)
        {
            if (Mathf.Abs(amount) < 0.01f) return;

            // Apply force in movement direction
            Vector3 direction = wheelchair.Velocity.normalized;
            if (direction.magnitude < 0.1f)
            {
                direction = wheelchair.transform.forward;
            }

            wheelchair.ApplyImpulse(direction * amount * Time.fixedDeltaTime);
        }

        private void ApplyDeceleration(Player.WheelchairController wheelchair, float amount)
        {
            if (wheelchair.CurrentSpeed < 0.5f) return;

            Vector3 brakeDirection = -wheelchair.Velocity.normalized;
            wheelchair.ApplyImpulse(brakeDirection * Mathf.Abs(amount) * Time.fixedDeltaTime);
        }

        private void ApplyBoost(Player.WheelchairController wheelchair)
        {
            // Apply boost in ramp direction
            Vector3 boostDirection = transform.forward;
            wheelchair.ApplyImpulse(boostDirection * boostForce);

            // Visual/audio feedback
            if (boostParticles != null)
            {
                boostParticles.Play();
            }

            if (boostSound != null)
            {
                boostSound.Play();
            }
        }

        /// <summary>
        /// Get the ramp's slope angle
        /// </summary>
        public float GetAngle()
        {
            return angle;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ramp direction
            Gizmos.color = rampType switch
            {
                RampType.Boost => Color.green,
                RampType.Slippery => Color.cyan,
                RampType.Sticky => Color.magenta,
                _ => Color.yellow
            };

            Vector3 start = transform.position;
            Vector3 end = start + transform.forward * 3f;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.2f);

            // Draw angle indicator
            Vector3 horizontal = start + Vector3.forward * 3f;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(start, horizontal);
        }
    }
}
