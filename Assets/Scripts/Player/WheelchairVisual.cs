using UnityEngine;

namespace WheelchairRacing.Player
{
    /// <summary>
    /// Creates a wheelchair visual from Unity primitives.
    /// Attach to the wheelchair GameObject to generate the visual.
    /// Works in both Edit mode and Play mode.
    /// </summary>
    [ExecuteAlways]
    public class WheelchairVisual : MonoBehaviour
    {
        [Header("Wheelchair Colors")]
        [SerializeField] private Color frameColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color seatColor = new Color(0.1f, 0.1f, 0.4f);
        [SerializeField] private Color wheelColor = new Color(0.15f, 0.15f, 0.15f);
        [SerializeField] private Color tireColor = new Color(0.05f, 0.05f, 0.05f);
        [SerializeField] private Color spokesColor = new Color(0.6f, 0.6f, 0.6f);

        [Header("Rider Colors")]
        [SerializeField] private Color skinColor = new Color(0.96f, 0.8f, 0.69f);
        [SerializeField] private Color bodyColor = new Color(0.9f, 0.5f, 0.1f);
        [SerializeField] private Color pantsColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color shoeColor = new Color(0.15f, 0.15f, 0.15f);

        [Header("Dimensions")]
        [SerializeField] private float seatWidth = 0.5f;
        [SerializeField] private float seatDepth = 0.45f;
        [SerializeField] private float seatHeight = 0.05f;
        [SerializeField] private float seatElevation = 0.55f;

        [SerializeField] private float backrestHeight = 0.4f;
        [SerializeField] private float backrestThickness = 0.05f;

        [SerializeField] private float bigWheelRadius = 0.35f;
        [SerializeField] private float bigWheelWidth = 0.04f;
        [SerializeField] private float smallWheelRadius = 0.08f;
        [SerializeField] private float smallWheelWidth = 0.03f;

        [SerializeField] private float armrestHeight = 0.15f;
        [SerializeField] private float armrestLength = 0.35f;

        [SerializeField] private float footrestWidth = 0.3f;
        [SerializeField] private float footrestDepth = 0.15f;

        private GameObject visualRoot;
        private Material frameMaterial;
        private Material seatMaterial;
        private Material wheelMaterial;
        private Material tireMaterial;
        private Material spokesMaterial;
        private Material skinMaterial;
        private Material bodyMaterial;
        private Material pantsMaterial;
        private Material shoeMaterial;

        private void OnEnable()
        {
            // Check if visual already exists (might be loaded from scene)
            var existingVisual = transform.Find("WheelchairVisual");
            if (existingVisual != null)
            {
                visualRoot = existingVisual.gameObject;
                return;
            }

            CreateMaterials();
            CreateWheelchairVisual();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // Regenerate when properties change in editor
            if (!Application.isPlaying && visualRoot != null)
            {
                // Delay the rebuild to avoid issues during validation
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null)
                    {
                        RegenerateVisual();
                    }
                };
            }
#endif
        }

        [ContextMenu("Regenerate Wheelchair Visual")]
        public void RegenerateVisual()
        {
            DestroyVisual();
            CreateMaterials();
            CreateWheelchairVisual();
        }

        private void DestroyVisual()
        {
            if (visualRoot != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(visualRoot);
                }
                else
                {
                    DestroyImmediate(visualRoot);
                }
                visualRoot = null;
            }

            CleanupMaterials();
        }

        private void CleanupMaterials()
        {
            void SafeDestroy(Material mat)
            {
                if (mat != null)
                {
                    if (Application.isPlaying)
                        Destroy(mat);
                    else
                        DestroyImmediate(mat);
                }
            }

            SafeDestroy(frameMaterial);
            SafeDestroy(seatMaterial);
            SafeDestroy(wheelMaterial);
            SafeDestroy(tireMaterial);
            SafeDestroy(spokesMaterial);
            SafeDestroy(skinMaterial);
            SafeDestroy(bodyMaterial);
            SafeDestroy(pantsMaterial);
            SafeDestroy(shoeMaterial);

            frameMaterial = null;
            seatMaterial = null;
            wheelMaterial = null;
            tireMaterial = null;
            spokesMaterial = null;
            skinMaterial = null;
            bodyMaterial = null;
            pantsMaterial = null;
            shoeMaterial = null;
        }

        private void CreateMaterials()
        {
            // Get the default material from a primitive - this always uses the correct shader
            var tempPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var defaultMat = tempPrimitive.GetComponent<MeshRenderer>().sharedMaterial;

            if (Application.isPlaying)
                Destroy(tempPrimitive);
            else
                DestroyImmediate(tempPrimitive);

            if (defaultMat == null) return;

            // Create new material instances based on the default
            frameMaterial = new Material(defaultMat);
            seatMaterial = new Material(defaultMat);
            wheelMaterial = new Material(defaultMat);
            tireMaterial = new Material(defaultMat);
            spokesMaterial = new Material(defaultMat);
            skinMaterial = new Material(defaultMat);
            bodyMaterial = new Material(defaultMat);
            pantsMaterial = new Material(defaultMat);
            shoeMaterial = new Material(defaultMat);

            // Set wheelchair colors
            SetMaterialColor(frameMaterial, frameColor);
            SetMaterialColor(seatMaterial, seatColor);
            SetMaterialColor(wheelMaterial, wheelColor);
            SetMaterialColor(tireMaterial, tireColor);
            SetMaterialColor(spokesMaterial, spokesColor);

            // Set rider colors
            SetMaterialColor(skinMaterial, skinColor);
            SetMaterialColor(bodyMaterial, bodyColor);
            SetMaterialColor(pantsMaterial, pantsColor);
            SetMaterialColor(shoeMaterial, shoeColor);
        }

        private void SetMaterialColor(Material mat, Color color)
        {
            // Try all common color property names
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            mat.color = color;
        }

        private void CreateWheelchairVisual()
        {
            if (frameMaterial == null) return;

            visualRoot = new GameObject("WheelchairVisual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = Vector3.zero;

            CreateSeat();
            CreateBackrest();
            CreateArmrests();
            CreateFootrest();
            CreateBigWheels();
            CreateSmallWheels();
            CreateFrame();
            CreateRider();
        }

        private void CreateSeat()
        {
            var seat = CreatePrimitive("Seat", PrimitiveType.Cube, seatMaterial);
            seat.transform.localPosition = new Vector3(0f, seatElevation, 0f);
            seat.transform.localScale = new Vector3(seatWidth, seatHeight, seatDepth);
        }

        private void CreateBackrest()
        {
            var backrest = CreatePrimitive("Backrest", PrimitiveType.Cube, seatMaterial);
            float backZ = -seatDepth / 2f + backrestThickness / 2f;
            float backY = seatElevation + seatHeight / 2f + backrestHeight / 2f;
            backrest.transform.localPosition = new Vector3(0f, backY, backZ);
            backrest.transform.localScale = new Vector3(seatWidth, backrestHeight, backrestThickness);
        }

        private void CreateArmrests()
        {
            float armY = seatElevation + seatHeight / 2f + armrestHeight / 2f;
            float armX = seatWidth / 2f + 0.02f;
            float armZ = seatDepth / 2f - armrestLength / 2f;

            var leftArm = CreatePrimitive("LeftArmrest", PrimitiveType.Cube, frameMaterial);
            leftArm.transform.localPosition = new Vector3(-armX, armY, armZ);
            leftArm.transform.localScale = new Vector3(0.03f, armrestHeight, armrestLength);

            var rightArm = CreatePrimitive("RightArmrest", PrimitiveType.Cube, frameMaterial);
            rightArm.transform.localPosition = new Vector3(armX, armY, armZ);
            rightArm.transform.localScale = new Vector3(0.03f, armrestHeight, armrestLength);

            float padY = armY + armrestHeight / 2f + 0.015f;

            var leftPad = CreatePrimitive("LeftArmrestPad", PrimitiveType.Cube, seatMaterial);
            leftPad.transform.localPosition = new Vector3(-armX, padY, armZ);
            leftPad.transform.localScale = new Vector3(0.06f, 0.03f, armrestLength);

            var rightPad = CreatePrimitive("RightArmrestPad", PrimitiveType.Cube, seatMaterial);
            rightPad.transform.localPosition = new Vector3(armX, padY, armZ);
            rightPad.transform.localScale = new Vector3(0.06f, 0.03f, armrestLength);
        }

        private void CreateFootrest()
        {
            var footrest = CreatePrimitive("Footrest", PrimitiveType.Cube, frameMaterial);
            float footZ = seatDepth / 2f + 0.15f;
            float footY = 0.15f;
            footrest.transform.localPosition = new Vector3(0f, footY, footZ);
            footrest.transform.localScale = new Vector3(footrestWidth, 0.02f, footrestDepth);

            var leftSupport = CreatePrimitive("LeftFootSupport", PrimitiveType.Cylinder, frameMaterial);
            leftSupport.transform.localPosition = new Vector3(-footrestWidth / 3f, 0.25f, footZ - 0.05f);
            leftSupport.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
            leftSupport.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            var rightSupport = CreatePrimitive("RightFootSupport", PrimitiveType.Cylinder, frameMaterial);
            rightSupport.transform.localPosition = new Vector3(footrestWidth / 3f, 0.25f, footZ - 0.05f);
            rightSupport.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);
            rightSupport.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
        }

        private void CreateBigWheels()
        {
            float wheelX = seatWidth / 2f + bigWheelWidth / 2f + 0.05f;
            float wheelY = bigWheelRadius;
            float wheelZ = -seatDepth / 4f;

            CreateWheel("LeftBigWheel", new Vector3(-wheelX, wheelY, wheelZ), bigWheelRadius, bigWheelWidth, true);
            CreateWheel("RightBigWheel", new Vector3(wheelX, wheelY, wheelZ), bigWheelRadius, bigWheelWidth, true);
        }

        private void CreateSmallWheels()
        {
            float wheelX = seatWidth / 2f - 0.05f;
            float wheelY = smallWheelRadius;
            float wheelZ = seatDepth / 2f + 0.1f;

            CreateCasterWheel("LeftCaster", new Vector3(-wheelX, wheelY, wheelZ));
            CreateCasterWheel("RightCaster", new Vector3(wheelX, wheelY, wheelZ));
        }

        private void CreateWheel(string name, Vector3 position, float radius, float width, bool addSpokes)
        {
            var wheelParent = new GameObject(name);
            wheelParent.transform.SetParent(visualRoot.transform, false);
            wheelParent.transform.localPosition = position;

            var tire = CreatePrimitive(name + "_Tire", PrimitiveType.Cylinder, tireMaterial);
            tire.transform.SetParent(wheelParent.transform, false);
            tire.transform.localPosition = Vector3.zero;
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tire.transform.localScale = new Vector3(radius * 2f, width / 2f, radius * 2f);

            var hub = CreatePrimitive(name + "_Hub", PrimitiveType.Cylinder, spokesMaterial);
            hub.transform.SetParent(wheelParent.transform, false);
            hub.transform.localPosition = Vector3.zero;
            hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            hub.transform.localScale = new Vector3(radius * 0.3f, width / 2f + 0.01f, radius * 0.3f);

            if (addSpokes)
            {
                int spokeCount = 8;
                for (int i = 0; i < spokeCount; i++)
                {
                    float angle = (360f / spokeCount) * i;
                    var spoke = CreatePrimitive(name + "_Spoke" + i, PrimitiveType.Cube, spokesMaterial);
                    spoke.transform.SetParent(wheelParent.transform, false);
                    spoke.transform.localRotation = Quaternion.Euler(angle, 0f, 0f);
                    spoke.transform.localPosition = Vector3.zero;
                    spoke.transform.localScale = new Vector3(0.01f, radius * 0.85f, 0.01f);
                }
            }

            if (radius > 0.2f)
            {
                var handRim = CreatePrimitive(name + "_HandRim", PrimitiveType.Cylinder, spokesMaterial);
                handRim.transform.SetParent(wheelParent.transform, false);
                handRim.transform.localPosition = new Vector3(position.x > 0 ? 0.02f : -0.02f, 0f, 0f);
                handRim.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                handRim.transform.localScale = new Vector3(radius * 1.6f, 0.01f, radius * 1.6f);
            }
        }

        private void CreateCasterWheel(string name, Vector3 position)
        {
            var casterParent = new GameObject(name);
            casterParent.transform.SetParent(visualRoot.transform, false);
            casterParent.transform.localPosition = position;

            var fork = CreatePrimitive(name + "_Fork", PrimitiveType.Cube, frameMaterial);
            fork.transform.SetParent(casterParent.transform, false);
            fork.transform.localPosition = new Vector3(0f, smallWheelRadius + 0.03f, 0f);
            fork.transform.localScale = new Vector3(0.02f, 0.06f, 0.02f);

            var wheel = CreatePrimitive(name + "_Wheel", PrimitiveType.Cylinder, tireMaterial);
            wheel.transform.SetParent(casterParent.transform, false);
            wheel.transform.localPosition = Vector3.zero;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(smallWheelRadius * 2f, smallWheelWidth / 2f, smallWheelRadius * 2f);
        }

        private void CreateFrame()
        {
            float tubeY = seatElevation - 0.05f;

            var leftTube = CreatePrimitive("LeftSeatTube", PrimitiveType.Cylinder, frameMaterial);
            leftTube.transform.localPosition = new Vector3(-seatWidth / 2f + 0.05f, tubeY, 0f);
            leftTube.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftTube.transform.localScale = new Vector3(0.025f, seatDepth / 2f + 0.1f, 0.025f);

            var rightTube = CreatePrimitive("RightSeatTube", PrimitiveType.Cylinder, frameMaterial);
            rightTube.transform.localPosition = new Vector3(seatWidth / 2f - 0.05f, tubeY, 0f);
            rightTube.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightTube.transform.localScale = new Vector3(0.025f, seatDepth / 2f + 0.1f, 0.025f);

            float wheelX = seatWidth / 2f + bigWheelWidth / 2f + 0.05f;
            float wheelZ = -seatDepth / 4f;

            var leftVertical = CreatePrimitive("LeftVerticalTube", PrimitiveType.Cylinder, frameMaterial);
            leftVertical.transform.localPosition = new Vector3(-wheelX + 0.03f, seatElevation / 2f + 0.1f, wheelZ);
            leftVertical.transform.localScale = new Vector3(0.025f, seatElevation / 2f, 0.025f);

            var rightVertical = CreatePrimitive("RightVerticalTube", PrimitiveType.Cylinder, frameMaterial);
            rightVertical.transform.localPosition = new Vector3(wheelX - 0.03f, seatElevation / 2f + 0.1f, wheelZ);
            rightVertical.transform.localScale = new Vector3(0.025f, seatElevation / 2f, 0.025f);

            var crossTube = CreatePrimitive("CrossTube", PrimitiveType.Cylinder, frameMaterial);
            crossTube.transform.localPosition = new Vector3(0f, bigWheelRadius, wheelZ);
            crossTube.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            crossTube.transform.localScale = new Vector3(0.025f, seatWidth / 2f + 0.1f, 0.025f);

            float casterZ = seatDepth / 2f + 0.1f;

            var leftFrontTube = CreatePrimitive("LeftFrontTube", PrimitiveType.Cylinder, frameMaterial);
            leftFrontTube.transform.localPosition = new Vector3(-seatWidth / 2f + 0.1f, tubeY - 0.1f, casterZ / 2f);
            leftFrontTube.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            leftFrontTube.transform.localScale = new Vector3(0.02f, 0.25f, 0.02f);

            var rightFrontTube = CreatePrimitive("RightFrontTube", PrimitiveType.Cylinder, frameMaterial);
            rightFrontTube.transform.localPosition = new Vector3(seatWidth / 2f - 0.1f, tubeY - 0.1f, casterZ / 2f);
            rightFrontTube.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            rightFrontTube.transform.localScale = new Vector3(0.02f, 0.25f, 0.02f);

            float handleY = seatElevation + seatHeight / 2f + backrestHeight + 0.15f;
            float handleZ = -seatDepth / 2f - 0.05f;

            var leftHandle = CreatePrimitive("LeftPushHandle", PrimitiveType.Cylinder, frameMaterial);
            leftHandle.transform.localPosition = new Vector3(-seatWidth / 2f + 0.05f, handleY - 0.1f, handleZ);
            leftHandle.transform.localScale = new Vector3(0.02f, 0.2f, 0.02f);

            var rightHandle = CreatePrimitive("RightPushHandle", PrimitiveType.Cylinder, frameMaterial);
            rightHandle.transform.localPosition = new Vector3(seatWidth / 2f - 0.05f, handleY - 0.1f, handleZ);
            rightHandle.transform.localScale = new Vector3(0.02f, 0.2f, 0.02f);

            var leftGrip = CreatePrimitive("LeftGrip", PrimitiveType.Capsule, seatMaterial);
            leftGrip.transform.localPosition = new Vector3(-seatWidth / 2f + 0.05f, handleY, handleZ);
            leftGrip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftGrip.transform.localScale = new Vector3(0.03f, 0.05f, 0.03f);

            var rightGrip = CreatePrimitive("RightGrip", PrimitiveType.Capsule, seatMaterial);
            rightGrip.transform.localPosition = new Vector3(seatWidth / 2f - 0.05f, handleY, handleZ);
            rightGrip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightGrip.transform.localScale = new Vector3(0.03f, 0.05f, 0.03f);
        }

        private void CreateRider()
        {
            // Create a parent for the rider
            var riderParent = new GameObject("Rider");
            riderParent.transform.SetParent(visualRoot.transform, false);
            riderParent.transform.localPosition = Vector3.zero;

            // Rider dimensions
            float hipY = seatElevation + seatHeight / 2f + 0.08f;
            float torsoHeight = 0.35f;
            float torsoWidth = 0.25f;
            float headRadius = 0.1f;
            float limbThickness = 0.06f;

            // === TORSO ===
            var torso = CreatePrimitive("Torso", PrimitiveType.Capsule, bodyMaterial);
            torso.transform.SetParent(riderParent.transform, false);
            float torsoY = hipY + torsoHeight / 2f;
            torso.transform.localPosition = new Vector3(0f, torsoY, -0.02f);
            torso.transform.localScale = new Vector3(torsoWidth, torsoHeight / 2f, 0.15f);

            // === HEAD ===
            float neckY = torsoY + torsoHeight / 2f;

            // Neck
            var neck = CreatePrimitive("Neck", PrimitiveType.Cylinder, skinMaterial);
            neck.transform.SetParent(riderParent.transform, false);
            neck.transform.localPosition = new Vector3(0f, neckY + 0.03f, -0.02f);
            neck.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);

            // Head
            var head = CreatePrimitive("Head", PrimitiveType.Sphere, skinMaterial);
            head.transform.SetParent(riderParent.transform, false);
            head.transform.localPosition = new Vector3(0f, neckY + 0.06f + headRadius, -0.02f);
            head.transform.localScale = new Vector3(headRadius * 2f, headRadius * 2.2f, headRadius * 2f);

            // === ARMS ===
            float shoulderY = torsoY + torsoHeight / 3f;
            float shoulderX = torsoWidth / 2f + 0.02f;
            float upperArmLength = 0.18f;
            float lowerArmLength = 0.18f;

            // Upper arms (angled down to armrests)
            var leftUpperArm = CreatePrimitive("LeftUpperArm", PrimitiveType.Capsule, bodyMaterial);
            leftUpperArm.transform.SetParent(riderParent.transform, false);
            leftUpperArm.transform.localPosition = new Vector3(-shoulderX - 0.06f, shoulderY - 0.08f, 0f);
            leftUpperArm.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
            leftUpperArm.transform.localScale = new Vector3(limbThickness, upperArmLength / 2f, limbThickness);

            var rightUpperArm = CreatePrimitive("RightUpperArm", PrimitiveType.Capsule, bodyMaterial);
            rightUpperArm.transform.SetParent(riderParent.transform, false);
            rightUpperArm.transform.localPosition = new Vector3(shoulderX + 0.06f, shoulderY - 0.08f, 0f);
            rightUpperArm.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
            rightUpperArm.transform.localScale = new Vector3(limbThickness, upperArmLength / 2f, limbThickness);

            // Lower arms (on armrests, pointing forward)
            float armrestY = seatElevation + seatHeight / 2f + armrestHeight / 2f + 0.04f;
            float armX = seatWidth / 2f + 0.02f;

            var leftLowerArm = CreatePrimitive("LeftLowerArm", PrimitiveType.Capsule, skinMaterial);
            leftLowerArm.transform.SetParent(riderParent.transform, false);
            leftLowerArm.transform.localPosition = new Vector3(-armX, armrestY, 0.08f);
            leftLowerArm.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftLowerArm.transform.localScale = new Vector3(limbThickness * 0.9f, lowerArmLength / 2f, limbThickness * 0.9f);

            var rightLowerArm = CreatePrimitive("RightLowerArm", PrimitiveType.Capsule, skinMaterial);
            rightLowerArm.transform.SetParent(riderParent.transform, false);
            rightLowerArm.transform.localPosition = new Vector3(armX, armrestY, 0.08f);
            rightLowerArm.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightLowerArm.transform.localScale = new Vector3(limbThickness * 0.9f, lowerArmLength / 2f, limbThickness * 0.9f);

            // Hands (spheres at end of arms)
            var leftHand = CreatePrimitive("LeftHand", PrimitiveType.Sphere, skinMaterial);
            leftHand.transform.SetParent(riderParent.transform, false);
            leftHand.transform.localPosition = new Vector3(-armX, armrestY, 0.18f);
            leftHand.transform.localScale = new Vector3(0.06f, 0.04f, 0.08f);

            var rightHand = CreatePrimitive("RightHand", PrimitiveType.Sphere, skinMaterial);
            rightHand.transform.SetParent(riderParent.transform, false);
            rightHand.transform.localPosition = new Vector3(armX, armrestY, 0.18f);
            rightHand.transform.localScale = new Vector3(0.06f, 0.04f, 0.08f);

            // === LEGS ===
            float thighLength = 0.25f;
            float shinLength = 0.25f;
            float legSpacing = 0.1f;

            // Thighs (horizontal on seat)
            var leftThigh = CreatePrimitive("LeftThigh", PrimitiveType.Capsule, pantsMaterial);
            leftThigh.transform.SetParent(riderParent.transform, false);
            leftThigh.transform.localPosition = new Vector3(-legSpacing, hipY, seatDepth / 4f);
            leftThigh.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftThigh.transform.localScale = new Vector3(limbThickness * 1.2f, thighLength / 2f, limbThickness * 1.2f);

            var rightThigh = CreatePrimitive("RightThigh", PrimitiveType.Capsule, pantsMaterial);
            rightThigh.transform.SetParent(riderParent.transform, false);
            rightThigh.transform.localPosition = new Vector3(legSpacing, hipY, seatDepth / 4f);
            rightThigh.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightThigh.transform.localScale = new Vector3(limbThickness * 1.2f, thighLength / 2f, limbThickness * 1.2f);

            // Knees (at front of seat)
            float kneeY = hipY - 0.02f;
            float kneeZ = seatDepth / 2f + 0.02f;

            // Shins (going down from knees to footrest)
            float footrestY = 0.15f;
            float shinY = (kneeY + footrestY) / 2f + 0.05f;
            float shinAngle = 15f; // Slight forward angle

            var leftShin = CreatePrimitive("LeftShin", PrimitiveType.Capsule, pantsMaterial);
            leftShin.transform.SetParent(riderParent.transform, false);
            leftShin.transform.localPosition = new Vector3(-legSpacing, shinY, kneeZ + 0.05f);
            leftShin.transform.localRotation = Quaternion.Euler(shinAngle, 0f, 0f);
            leftShin.transform.localScale = new Vector3(limbThickness, shinLength / 2f, limbThickness);

            var rightShin = CreatePrimitive("RightShin", PrimitiveType.Capsule, pantsMaterial);
            rightShin.transform.SetParent(riderParent.transform, false);
            rightShin.transform.localPosition = new Vector3(legSpacing, shinY, kneeZ + 0.05f);
            rightShin.transform.localRotation = Quaternion.Euler(shinAngle, 0f, 0f);
            rightShin.transform.localScale = new Vector3(limbThickness, shinLength / 2f, limbThickness);

            // Feet (on footrest)
            float footY = footrestY + 0.03f;
            float footZ = seatDepth / 2f + 0.15f;

            var leftFoot = CreatePrimitive("LeftFoot", PrimitiveType.Cube, shoeMaterial);
            leftFoot.transform.SetParent(riderParent.transform, false);
            leftFoot.transform.localPosition = new Vector3(-legSpacing, footY, footZ + 0.03f);
            leftFoot.transform.localScale = new Vector3(0.07f, 0.05f, 0.12f);

            var rightFoot = CreatePrimitive("RightFoot", PrimitiveType.Cube, shoeMaterial);
            rightFoot.transform.SetParent(riderParent.transform, false);
            rightFoot.transform.localPosition = new Vector3(legSpacing, footY, footZ + 0.03f);
            rightFoot.transform.localScale = new Vector3(0.07f, 0.05f, 0.12f);
        }

        private GameObject CreatePrimitive(string name, PrimitiveType type, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(visualRoot.transform, false);

            var collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                    Destroy(collider);
                else
                    DestroyImmediate(collider);
            }

            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }

            return obj;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                DestroyVisual();
            }
        }

        private void OnDestroy()
        {
            DestroyVisual();
        }
    }
}
