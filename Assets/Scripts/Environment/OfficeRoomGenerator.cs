using UnityEngine;

namespace VRInterviewPractice.Environment
{
    /// <summary>
    /// Procedurally builds a clean, executive 3D interview office room using standard Unity primitives.
    /// Completely free of external asset store dependencies.
    /// </summary>
    public class OfficeRoomGenerator : MonoBehaviour
    {
        [Header("Room Generation Options")]
        [SerializeField] private bool generateOnStart = false;
        [SerializeField] private Vector3 roomDimensions = new Vector3(8f, 3.5f, 9f);

        [Header("References (Auto-Populated)")]
        public GameObject interviewerInstance;
        public Transform candidateSeatPoint;

        private void Start()
        {
            if (generateOnStart && transform.childCount == 0)
            {
                BuildCompleteOffice();
            }
        }

        [ContextMenu("Build Office Room")]
        public void BuildCompleteOffice()
        {
            // Clear existing children if regenerating in Editor
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 1. Shaders & Material Helpers
            Material floorMat = CreateColoredMaterial("Mat_CarpetFloor", new Color(0.18f, 0.20f, 0.25f)); // Dark corporate carpet
            Material wallMat = CreateColoredMaterial("Mat_OfficeWall", new Color(0.92f, 0.92f, 0.90f));    // Warm off-white
            Material deskMat = CreateColoredMaterial("Mat_MahoganyDesk", new Color(0.32f, 0.17f, 0.10f)); // Rich wood brown
            Material chairMat = CreateColoredMaterial("Mat_OfficeChair", new Color(0.12f, 0.12f, 0.14f)); // Modern charcoal black
            Material laptopMat = CreateColoredMaterial("Mat_LaptopBody", new Color(0.75f, 0.77f, 0.80f)); // Brushed aluminum
            Material screenMat = CreateColoredMaterial("Mat_LaptopScreen", new Color(0.15f, 0.35f, 0.60f), isEmissive: true);
            Material suitMat = CreateColoredMaterial("Mat_InterviewerSuit", new Color(0.15f, 0.18f, 0.24f)); // Navy suit
            Material skinMat = CreateColoredMaterial("Mat_InterviewerSkin", new Color(0.85f, 0.70f, 0.60f)); // Neutral skin tone
            Material whiteBoardMat = CreateColoredMaterial("Mat_Whiteboard", new Color(0.98f, 0.98f, 0.98f));

            // 2. Room Shell (Floor, Ceiling, Walls)
            CreateRoomShell(floorMat, wallMat);

            // 3. Furniture (Executive Desk, Candidate Chair, Interviewer Chair)
            CreateFurniture(deskMat, chairMat, laptopMat, screenMat);

            // 4. Whiteboard & Office Details
            CreateWhiteboard(whiteBoardMat);

            // 5. Stylized 3D Virtual Interviewer Avatar
            CreateInterviewerAvatar(suitMat, skinMat);

            // 6. Lighting Setup
            CreateOfficeLighting();

            Debug.Log("[OfficeRoomGenerator] Executive 3D interview room successfully constructed!");
        }

        private void CreateRoomShell(Material floorMat, Material wallMat)
        {
            GameObject roomParent = new GameObject("Room_Shell");
            roomParent.transform.SetParent(transform);

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(roomParent.transform);
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(roomDimensions.x, 0.2f, roomDimensions.z);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Ceiling
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(roomParent.transform);
            ceiling.transform.position = new Vector3(0, roomDimensions.y + 0.1f, 0);
            ceiling.transform.localScale = new Vector3(roomDimensions.x, 0.2f, roomDimensions.z);
            ceiling.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Back Wall (Behind Interviewer)
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.name = "Wall_Back";
            backWall.transform.SetParent(roomParent.transform);
            backWall.transform.position = new Vector3(0, roomDimensions.y / 2f, roomDimensions.z / 2f);
            backWall.transform.localScale = new Vector3(roomDimensions.x, roomDimensions.y, 0.2f);
            backWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Front Wall (Behind Player)
            GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWall.name = "Wall_Front";
            frontWall.transform.SetParent(roomParent.transform);
            frontWall.transform.position = new Vector3(0, roomDimensions.y / 2f, -roomDimensions.z / 2f);
            frontWall.transform.localScale = new Vector3(roomDimensions.x, roomDimensions.y, 0.2f);
            frontWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Left Wall
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "Wall_Left";
            leftWall.transform.SetParent(roomParent.transform);
            leftWall.transform.position = new Vector3(-roomDimensions.x / 2f, roomDimensions.y / 2f, 0);
            leftWall.transform.localScale = new Vector3(0.2f, roomDimensions.y, roomDimensions.z);
            leftWall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Right Wall
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "Wall_Right";
            rightWall.transform.SetParent(roomParent.transform);
            rightWall.transform.position = new Vector3(roomDimensions.x / 2f, roomDimensions.y / 2f, 0);
            rightWall.transform.localScale = new Vector3(0.2f, roomDimensions.y, roomDimensions.z);
            rightWall.GetComponent<Renderer>().sharedMaterial = wallMat;
        }

        private void CreateFurniture(Material deskMat, Material chairMat, Material laptopMat, Material screenMat)
        {
            GameObject furnParent = new GameObject("Furniture");
            furnParent.transform.SetParent(transform);

            // --- Executive Interview Desk ---
            GameObject desk = new GameObject("Interview_Desk");
            desk.transform.SetParent(furnParent.transform);
            desk.transform.position = new Vector3(0, 0, 0.5f);

            // Tabletop
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "TableTop";
            top.transform.SetParent(desk.transform);
            top.transform.localPosition = new Vector3(0, 0.75f, 0);
            top.transform.localScale = new Vector3(2.4f, 0.08f, 1.2f);
            top.GetComponent<Renderer>().sharedMaterial = deskMat;

            // Table Legs
            Vector3[] legOffsets = new Vector3[]
            {
                new Vector3(-1.1f, 0.35f, -0.5f),
                new Vector3(1.1f, 0.35f, -0.5f),
                new Vector3(-1.1f, 0.35f, 0.5f),
                new Vector3(1.1f, 0.35f, 0.5f)
            };
            foreach (var offset in legOffsets)
            {
                GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "DeskLeg";
                leg.transform.SetParent(desk.transform);
                leg.transform.localPosition = offset;
                leg.transform.localScale = new Vector3(0.08f, 0.37f, 0.08f);
                leg.GetComponent<Renderer>().sharedMaterial = chairMat;
            }

            // --- Laptop on Desk ---
            GameObject laptop = new GameObject("Laptop");
            laptop.transform.SetParent(desk.transform);
            laptop.transform.localPosition = new Vector3(0.4f, 0.80f, 0.1f);
            laptop.transform.localRotation = Quaternion.Euler(0, -15f, 0);

            // Laptop base
            GameObject lapBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lapBase.name = "LaptopBase";
            lapBase.transform.SetParent(laptop.transform);
            lapBase.transform.localPosition = Vector3.zero;
            lapBase.transform.localScale = new Vector3(0.35f, 0.015f, 0.25f);
            lapBase.GetComponent<Renderer>().sharedMaterial = laptopMat;

            // Laptop lid/screen
            GameObject lapScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lapScreen.name = "LaptopScreen";
            lapScreen.transform.SetParent(laptop.transform);
            lapScreen.transform.localPosition = new Vector3(0, 0.10f, 0.11f);
            lapScreen.transform.localRotation = Quaternion.Euler(110f, 0, 0);
            lapScreen.transform.localScale = new Vector3(0.35f, 0.012f, 0.22f);
            lapScreen.GetComponent<Renderer>().sharedMaterial = screenMat;

            // --- Interviewer Chair (Far Side) ---
            CreateChair("Interviewer_Chair", furnParent.transform, new Vector3(0, 0, 1.4f), Quaternion.Euler(0, 180, 0), chairMat);

            // --- Candidate Seating Area (Near Side) ---
            GameObject candSeat = new GameObject("Candidate_Seat_Marker");
            candSeat.transform.SetParent(furnParent.transform);
            candSeat.transform.position = new Vector3(0, 1.25f, -0.6f);
            candidateSeatPoint = candSeat.transform;

            CreateChair("Candidate_Chair", furnParent.transform, new Vector3(0, 0, -0.7f), Quaternion.identity, chairMat);
        }

        private void CreateChair(string chairName, Transform parent, Vector3 pos, Quaternion rot, Material mat)
        {
            GameObject chair = new GameObject(chairName);
            chair.transform.SetParent(parent);
            chair.transform.position = pos;
            chair.transform.rotation = rot;

            // Seat cushion
            GameObject seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seat.name = "SeatCushion";
            seat.transform.SetParent(chair.transform);
            seat.transform.localPosition = new Vector3(0, 0.45f, 0);
            seat.transform.localScale = new Vector3(0.6f, 0.08f, 0.6f);
            seat.GetComponent<Renderer>().sharedMaterial = mat;

            // Backrest
            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backrest";
            back.transform.SetParent(chair.transform);
            back.transform.localPosition = new Vector3(0, 0.85f, -0.26f);
            back.transform.localScale = new Vector3(0.55f, 0.75f, 0.08f);
            back.GetComponent<Renderer>().sharedMaterial = mat;

            // Central pedestal stem
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.name = "Stem";
            stem.transform.SetParent(chair.transform);
            stem.transform.localPosition = new Vector3(0, 0.22f, 0);
            stem.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            stem.GetComponent<Renderer>().sharedMaterial = mat;

            // Base star legs
            GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "BasePlate";
            basePlate.transform.SetParent(chair.transform);
            basePlate.transform.localPosition = new Vector3(0, 0.02f, 0);
            basePlate.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
            basePlate.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void CreateWhiteboard(Material wbMat)
        {
            GameObject wb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wb.name = "Office_Whiteboard";
            wb.transform.SetParent(transform);
            wb.transform.position = new Vector3(-3.85f, 2.0f, 0.5f);
            wb.transform.localScale = new Vector3(0.05f, 1.4f, 2.4f);
            wb.GetComponent<Renderer>().sharedMaterial = wbMat;
        }

        private void CreateInterviewerAvatar(Material suitMat, Material skinMat)
        {
            GameObject avatar = new GameObject("Virtual_Interviewer");
            avatar.transform.SetParent(transform);
            avatar.transform.position = new Vector3(0, 0, 1.4f);
            avatar.transform.rotation = Quaternion.Euler(0, 180f, 0);

            // Torso (Suit Jacket)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Torso";
            torso.transform.SetParent(avatar.transform);
            torso.transform.localPosition = new Vector3(0, 0.85f, 0);
            torso.transform.localScale = new Vector3(0.48f, 0.55f, 0.28f);
            torso.GetComponent<Renderer>().sharedMaterial = suitMat;

            // Neck
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            neck.transform.SetParent(avatar.transform);
            neck.transform.localPosition = new Vector3(0, 1.16f, 0);
            neck.transform.localScale = new Vector3(0.12f, 0.08f, 0.12f);
            neck.GetComponent<Renderer>().sharedMaterial = skinMat;

            // Head (With subtle breathing/animation controller)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(avatar.transform);
            head.transform.localPosition = new Vector3(0, 1.34f, 0);
            head.transform.localScale = new Vector3(0.26f, 0.28f, 0.26f);
            head.GetComponent<Renderer>().sharedMaterial = skinMat;

            // Eye Contact Focal Point
            GameObject eyesMarker = new GameObject("EyeFocusPoint");
            eyesMarker.transform.SetParent(head.transform);
            eyesMarker.transform.localPosition = new Vector3(0, 0, 0.14f);

            // Tie Accent
            Material tieMat = CreateColoredMaterial("Mat_TieRed", new Color(0.70f, 0.12f, 0.15f));
            GameObject tie = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tie.name = "Tie";
            tie.transform.SetParent(torso.transform);
            tie.transform.localPosition = new Vector3(0, 0.05f, 0.15f);
            tie.transform.localScale = new Vector3(0.10f, 0.35f, 0.02f);
            tie.GetComponent<Renderer>().sharedMaterial = tieMat;

            // Arms resting forward on desk
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(avatar.transform);
            leftArm.transform.localPosition = new Vector3(-0.32f, 0.72f, 0.22f);
            leftArm.transform.localRotation = Quaternion.Euler(45f, 20f, 0);
            leftArm.transform.localScale = new Vector3(0.09f, 0.25f, 0.09f);
            leftArm.GetComponent<Renderer>().sharedMaterial = suitMat;

            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(avatar.transform);
            rightArm.transform.localPosition = new Vector3(0.32f, 0.72f, 0.22f);
            rightArm.transform.localRotation = Quaternion.Euler(45f, -20f, 0);
            rightArm.transform.localScale = new Vector3(0.09f, 0.25f, 0.09f);
            rightArm.GetComponent<Renderer>().sharedMaterial = suitMat;

            // Attach dynamic VirtualInterviewer controller for lifelike breathing & reactive nodding
            VirtualInterviewer interviewerCtrl = avatar.AddComponent<VirtualInterviewer>();
            interviewerCtrl.headTransform = head.transform;
            interviewerCtrl.torsoTransform = torso.transform;

            interviewerInstance = avatar;
        }

        private void CreateOfficeLighting()
        {
            GameObject lightsParent = new GameObject("Lighting");
            lightsParent.transform.SetParent(transform);

            // Soft Overhead Office Ceiling Light
            GameObject ceilingLightObj = new GameObject("Ceiling_Panel_Light");
            ceilingLightObj.transform.SetParent(lightsParent.transform);
            ceilingLightObj.transform.position = new Vector3(0, 3.2f, 0.5f);
            Light ceilingLight = ceilingLightObj.AddComponent<Light>();
            ceilingLight.type = LightType.Point;
            ceilingLight.color = new Color(1.0f, 0.96f, 0.90f); // Warm neutral white
            ceilingLight.intensity = 1.6f;
            ceilingLight.range = 10f;
            ceilingLight.shadows = LightShadows.Soft;

            // Soft Face Key Light for Interviewer
            GameObject keyLightObj = new GameObject("Interviewer_Key_Light");
            keyLightObj.transform.SetParent(lightsParent.transform);
            keyLightObj.transform.position = new Vector3(0.6f, 1.8f, 0.3f);
            Light keyLight = keyLightObj.AddComponent<Light>();
            keyLight.type = LightType.Point;
            keyLight.color = new Color(0.95f, 0.95f, 1.0f);
            keyLight.intensity = 0.8f;
            keyLight.range = 4f;
        }

               private Material CreateColoredMaterial(string name, Color color, bool isEmissive = false)
        {
            // Support both Unity 6 URP (Universal Render Pipeline) and Built-in RP
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Diffuse");

            Material mat = new Material(shader)
            {
                name = name
            };

            // Set color property compatible with both URP (_BaseColor) and Standard (_Color)
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (isEmissive)
            {
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", color * 0.8f);
                }
            }

            return mat;
        }
    }
}
