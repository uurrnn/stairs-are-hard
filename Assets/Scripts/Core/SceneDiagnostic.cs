using UnityEngine;

namespace WheelchairRacing.Core
{
    /// <summary>
    /// Diagnostic script to help fix common scene setup issues.
    /// Attach this to any GameObject and press Play to see diagnostics.
    /// </summary>
    public class SceneDiagnostic : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("========== SCENE DIAGNOSTIC START ==========");

            // Find the wheelchair
            var wheelchair = GameObject.Find("Wheelchair");
            if (wheelchair != null)
            {
                Debug.Log($"[Diagnostic] Wheelchair found at position: {wheelchair.transform.position}");

                // Check for required components
                var controller = wheelchair.GetComponent<Player.WheelchairController>();
                var input = wheelchair.GetComponent<Player.WheelchairInput>();
                var rb = wheelchair.GetComponent<Rigidbody>();

                Debug.Log($"[Diagnostic] WheelchairController: {(controller != null ? "FOUND" : "MISSING")}");
                Debug.Log($"[Diagnostic] WheelchairInput: {(input != null ? "FOUND" : "MISSING")}");
                Debug.Log($"[Diagnostic] Rigidbody: {(rb != null ? "FOUND" : "MISSING")}");

                // Check for GroundCheck child
                var groundCheck = wheelchair.transform.Find("GroundCheck");
                Debug.Log($"[Diagnostic] GroundCheck child: {(groundCheck != null ? $"FOUND at {groundCheck.position}" : "MISSING")}");
            }
            else
            {
                Debug.LogError("[Diagnostic] Wheelchair GameObject NOT FOUND in scene!");
            }

            // Find ground objects
            var groundObjects = GameObject.FindGameObjectsWithTag("Untagged");
            int groundCount = 0;
            foreach (var obj in groundObjects)
            {
                if (obj.name.ToLower().Contains("ground") || obj.name.ToLower().Contains("floor") || obj.name.ToLower().Contains("plane"))
                {
                    var collider = obj.GetComponent<Collider>();
                    Debug.Log($"[Diagnostic] Ground object '{obj.name}' at {obj.transform.position} | Has Collider: {collider != null} | Layer: {LayerMask.LayerToName(obj.layer)}");
                    groundCount++;
                }
            }

            if (groundCount == 0)
            {
                Debug.LogWarning("[Diagnostic] NO GROUND OBJECTS FOUND! You need to add ground to the scene.");
            }

            // Check distance from wheelchair to ground
            if (wheelchair != null)
            {
                if (Physics.Raycast(wheelchair.transform.position, Vector3.down, out RaycastHit hit, 100f))
                {
                    Debug.Log($"[Diagnostic] Wheelchair is {hit.distance:F2} units above '{hit.collider.gameObject.name}'");

                    if (hit.distance > 0.5f)
                    {
                        Debug.LogWarning($"[Diagnostic] WARNING: Wheelchair is too high! Needs to be < 0.3 units from ground. Current distance: {hit.distance:F2}");
                    }
                }
                else
                {
                    Debug.LogError("[Diagnostic] NO GROUND DETECTED BELOW WHEELCHAIR (checked 100 units down)!");
                }
            }

            Debug.Log("========== SCENE DIAGNOSTIC END ==========");
        }
    }
}
