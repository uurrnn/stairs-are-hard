using UnityEngine;

namespace WheelchairRacing.Level
{
    /// <summary>
    /// Trigger zone that respawns the player when they fall out of bounds.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FallZone : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string playerTag = "Player";

        [Header("Visual Feedback")]
        [SerializeField] private AudioSource fallSound;

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
            if (other.CompareTag(playerTag) || other.GetComponent<Player.WheelchairController>() != null)
            {
                TriggerFall(other.gameObject);
            }
        }

        private void TriggerFall(GameObject player)
        {
            // Play sound
            if (fallSound != null)
            {
                fallSound.Play();
            }

            // Notify GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.PlayerFell();
            }
            else
            {
                // Fallback: Reset player position manually
                var controller = player.GetComponent<Player.WheelchairController>();
                if (controller != null)
                {
                    controller.Teleport(Vector3.up * 5f, Quaternion.identity);
                }
                Debug.Log("Player fell! (GameManager not found)");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);

            var collider = GetComponent<Collider>();
            if (collider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
        }
    }
}
