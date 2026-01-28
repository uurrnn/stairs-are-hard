using UnityEngine;

namespace WheelchairRacing.Level
{
    /// <summary>
    /// Checkpoint trigger that saves player progress.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private int checkpointIndex = 0;
        [SerializeField] private string playerTag = "Player";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject inactiveVisual;
        [SerializeField] private GameObject activeVisual;
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private AudioSource activationSound;

        private bool isActivated;

        public int CheckpointIndex => checkpointIndex;
        public bool IsActivated => isActivated;

        private void Awake()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            UpdateVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActivated) return;

            if (other.CompareTag(playerTag) || other.GetComponent<Player.WheelchairController>() != null)
            {
                Activate();
            }
        }

        private void Activate()
        {
            isActivated = true;

            // Notify LevelManager
            if (Core.LevelManager.Instance != null)
            {
                Core.LevelManager.Instance.ActivateCheckpoint(checkpointIndex);
            }

            // Visual feedback
            UpdateVisuals();

            if (activationParticles != null)
            {
                activationParticles.Play();
            }

            if (activationSound != null)
            {
                activationSound.Play();
            }
        }

        private void UpdateVisuals()
        {
            if (inactiveVisual != null)
            {
                inactiveVisual.SetActive(!isActivated);
            }

            if (activeVisual != null)
            {
                activeVisual.SetActive(isActivated);
            }
        }

        /// <summary>
        /// Reset checkpoint state (for level restart)
        /// </summary>
        public void Reset()
        {
            isActivated = false;
            UpdateVisuals();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isActivated ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Draw checkpoint index
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Checkpoint {checkpointIndex}");
            #endif
        }
    }
}
