using UnityEngine;

namespace WheelchairRacing.Level
{
    /// <summary>
    /// Trigger zone that marks level completion when the player enters.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GoalZone : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem completionParticles;
        [SerializeField] private AudioSource completionSound;

        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered;

        private void Awake()
        {
            // Ensure collider is set as trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;

            if (other.CompareTag(playerTag) || other.GetComponent<Player.WheelchairController>() != null)
            {
                TriggerGoal();
            }
        }

        private void TriggerGoal()
        {
            hasTriggered = true;

            // Play effects
            if (completionParticles != null)
            {
                completionParticles.Play();
            }

            if (completionSound != null)
            {
                completionSound.Play();
            }

            // Notify GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.CompleteLevel();
            }
            else
            {
                Debug.Log("Goal reached! (GameManager not found)");
            }
        }

        /// <summary>
        /// Reset the goal zone (for level restart without scene reload)
        /// </summary>
        public void Reset()
        {
            hasTriggered = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);

            var collider = GetComponent<Collider>();
            if (collider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (collider is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}
