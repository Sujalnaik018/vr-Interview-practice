#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VRInterviewPractice.Audio;
using VRInterviewPractice.Core;
using VRInterviewPractice.Environment;
using VRInterviewPractice.UI;

namespace VRInterviewPractice.Editor
{
    /// <summary>
    /// One-click automated scene generator for the VR Interview Practice System.
    /// Builds the 3D office, virtual interviewer, UI Canvas, and hooks up all script references.
    /// </summary>
    public static class SetupInterviewScene
    {
        [MenuItem("Tools/VR Interview Practice/Generate Complete Scene", false, 1)]
        public static void GenerateScene()
        {
            if (!EditorUtility.DisplayDialog("Generate Complete Scene?",
                "This will build the 3D office room, the Canvas UI hierarchy, and link all system controllers in the active scene.\n\nContinue?",
                "Generate Scene", "Cancel"))
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Generate Interview Scene");

            // 1. Ensure EventSystem exists
            UnityEngine.EventSystems.EventSystem eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            // 2. Setup Candidate Seated Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camObj, "Create Main Camera");
            }

            mainCam.transform.position = new Vector3(0f, 1.25f, -0.6f);
            mainCam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 60f;

            if (mainCam.GetComponent<CameraLookController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraLookController>();
            }

            // 3. Build 3D Office Environment & Interviewer
            OfficeRoomGenerator roomGen = Object.FindObjectOfType<OfficeRoomGenerator>();
            if (roomGen == null)
            {
                GameObject roomObj = new GameObject("Office_Environment");
                roomGen = roomObj.AddComponent<OfficeRoomGenerator>();
                Undo.RegisterCreatedObjectUndo(roomObj, "Create Office Environment");
            }
            roomGen.BuildCompleteOffice();

            // 4. Create UI Canvas Hierarchy
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            GameObject canvasObj;
            if (canvas == null)
            {
                canvasObj = new GameObject("Interview_Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }
            else
            {
                canvasObj = canvas.gameObject;
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // --- Panel 1: Main Menu Panel ---
            GameObject menuPanel = CreateUIObject("MainMenu_Panel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 680), new Color(0.10f, 0.12f, 0.16f, 0.94f));
            MainMenuUI menuUI = menuPanel.AddComponent<MainMenuUI>();

            // Main Menu Title
            CreateText("TitleText", menuPanel.transform, "VR INTERVIEW PRACTICE SYSTEM", 34, FontStyle.Bold, Color.white, new Vector2(0, 270), new Vector2(800, 60), defaultFont);
            CreateText("SubtitleText", menuPanel.transform, "Simulated 3D Job Interview Environment", 18, FontStyle.Normal, new Color(0.7f, 0.8f, 0.9f), new Vector2(0, 230), new Vector2(800, 40), defaultFont);

            // Candidate Name Input
            CreateText("NameLabel", menuPanel.transform, "Enter Candidate Name:", 16, FontStyle.Normal, Color.white, new Vector2(-150, 160), new Vector2(300, 30), defaultFont, TextAnchor.MiddleLeft);
            InputField nameInput = CreateInputField("CandidateNameInput", menuPanel.transform, new Vector2(100, 160), new Vector2(250, 40), "Your Name", defaultFont);

            // Category Selection
            CreateText("CatLabel", menuPanel.transform, "Select Interview Track:", 16, FontStyle.Normal, Color.white, new Vector2(0, 90), new Vector2(600, 30), defaultFont);
            Button hrBtn = CreateButton("HR_Category_Btn", menuPanel.transform, "Behavioral & HR Track", new Vector2(-160, 40), new Vector2(280, 50), new Color(0.2f, 0.55f, 0.85f), defaultFont);
            Button techBtn = CreateButton("Tech_Category_Btn", menuPanel.transform, "Technical Systems Track", new Vector2(160, 40), new Vector2(280, 50), new Color(0.25f, 0.28f, 0.35f), defaultFont);
            Text catSelectedLabel = CreateText("SelectedCatLabel", menuPanel.transform, "Selected: Behavioral & HR Interview (5 Questions)", 14, FontStyle.Italic, new Color(0.8f, 0.8f, 0.8f), new Vector2(0, -5), new Vector2(700, 30), defaultFont);

            // Action Buttons
            Button startBtn = CreateButton("StartInterviewBtn", menuPanel.transform, "START PRACTICE SESSION", new Vector2(0, -75), new Vector2(360, 60), new Color(0.15f, 0.70f, 0.35f), defaultFont, 20);
            Button instructBtn = CreateButton("InstructionsBtn", menuPanel.transform, "Instructions & Scoring Guide", new Vector2(-140, -160), new Vector2(240, 45), new Color(0.35f, 0.40f, 0.48f), defaultFont, 14);
            Button historyBtn = CreateButton("HistoryBtn", menuPanel.transform, "View Past Sessions", new Vector2(140, -160), new Vector2(240, 45), new Color(0.35f, 0.40f, 0.48f), defaultFont, 14);
            Button exitBtn = CreateButton("ExitBtn", menuPanel.transform, "Exit Application", new Vector2(0, -225), new Vector2(200, 40), new Color(0.65f, 0.20f, 0.22f), defaultFont, 14);

            Text backendStatus = CreateText("BackendStatusText", menuPanel.transform, "Connecting to Flask backend...", 14, FontStyle.Normal, Color.yellow, new Vector2(0, -290), new Vector2(700, 30), defaultFont);

            // Instructions Modal Inside Menu
            GameObject instructModal = CreateUIObject("Instructions_Modal", menuPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 500), new Color(0.08f, 0.10f, 0.14f, 0.98f));
            CreateText("InstructHeader", instructModal.transform, "CANDIDATE INSTRUCTIONS & SCORING", 22, FontStyle.Bold, Color.white, new Vector2(0, 200), new Vector2(700, 40), defaultFont);
            string instructionsBody = "1. SUT UP & ENVIRONMENT:\n" +
                                      "   • Sit facing the 3D interviewer across the executive desk.\n" +
                                      "   • Hold right mouse button to look around your surroundings.\n\n" +
                                      "2. ANSWERING QUESTIONS:\n" +
                                      "   • Typed answers: Type directly into the response box.\n" +
                                      "   • Voice answers: Click 'Record Voice Answer', speak into your microphone, then click 'Stop Recording' to transcribe.\n\n" +
                                      "3. AI EVALUATION:\n" +
                                      "   • Answers are evaluated for Relevance (40%), Clarity (30%), and Completeness (30%).\n" +
                                      "   • Structured strengths and improvement recommendations are provided upon session completion.";
            CreateText("InstructBody", instructModal.transform, instructionsBody, 15, FontStyle.Normal, new Color(0.85f, 0.88f, 0.92f), new Vector2(0, 20), new Vector2(680, 280), defaultFont, TextAnchor.UpperLeft);
            Button closeInstructBtn = CreateButton("CloseInstructionsBtn", instructModal.transform, "Close Instructions", new Vector2(0, -190), new Vector2(220, 45), new Color(0.2f, 0.6f, 0.9f), defaultFont, 16);
            instructModal.SetActive(false);

            // Link MenuUI References
            SerializedObject menuSo = new SerializedObject(menuUI);
            menuSo.FindProperty("candidateNameInput").objectReferenceValue = nameInput;
            menuSo.FindProperty("hrCategoryButton").objectReferenceValue = hrBtn;
            menuSo.FindProperty("technicalCategoryButton").objectReferenceValue = techBtn;
            menuSo.FindProperty("selectedCategoryLabel").objectReferenceValue = catSelectedLabel;
            menuSo.FindProperty("startInterviewButton").objectReferenceValue = startBtn;
            menuSo.FindProperty("instructionsButton").objectReferenceValue = instructBtn;
            menuSo.FindProperty("historyButton").objectReferenceValue = historyBtn;
            menuSo.FindProperty("exitButton").objectReferenceValue = exitBtn;
            menuSo.FindProperty("instructionsModal").objectReferenceValue = instructModal;
            menuSo.FindProperty("closeInstructionsButton").objectReferenceValue = closeInstructBtn;
            menuSo.FindProperty("backendStatusText").objectReferenceValue = backendStatus;
            menuSo.ApplyModifiedProperties();

            // --- Panel 2: Interview HUD Panel ---
            GameObject interviewHUD = CreateUIObject("Interview_HUD_Panel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 780), new Color(0.08f, 0.10f, 0.14f, 0.92f));
            InterviewUI interviewScript = interviewHUD.AddComponent<InterviewUI>();

            Text qCounter = CreateText("QuestionCounter", interviewHUD.transform, "Question 1 of 5", 16, FontStyle.Bold, new Color(0.2f, 0.7f, 1.0f), new Vector2(0, 340), new Vector2(600, 30), defaultFont);
            Text qCategory = CreateText("QuestionCategory", interviewHUD.transform, "HR INTERVIEW", 13, FontStyle.Normal, Color.gray, new Vector2(0, 315), new Vector2(600, 25), defaultFont);
            Text qContent = CreateText("QuestionContent", interviewHUD.transform, "Tell me about yourself, your educational background, and your career interests.", 24, FontStyle.Bold, Color.white, new Vector2(0, 230), new Vector2(980, 100), defaultFont);
            Text qHint = CreateText("QuestionHint", interviewHUD.transform, "Hint: Structure your answer logically with relevant academic and project experience.", 14, FontStyle.Italic, new Color(0.7f, 0.75f, 0.8f), new Vector2(0, 160), new Vector2(980, 30), defaultFont);

            // Typed Answer Input Field
            InputField answerInput = CreateInputField("AnswerInputField", interviewHUD.transform, new Vector2(0, 30), new Vector2(980, 180), "Type your answer here or use the voice recording button below...", defaultFont, isMultiline: true);
            Text wordCount = CreateText("WordCount", interviewHUD.transform, "Word Count: 0", 13, FontStyle.Normal, Color.gray, new Vector2(400, -75), new Vector2(180, 25), defaultFont, TextAnchor.MiddleRight);

            // Voice Recording Controls
            Button voiceBtn = CreateButton("VoiceRecordBtn", interviewHUD.transform, "Record Voice Answer", new Vector2(-280, -140), new Vector2(260, 50), new Color(0.75f, 0.25f, 0.22f), defaultFont, 16);
            Text voiceBtnText = voiceBtn.GetComponentInChildren<Text>();

            GameObject recIndicatorObj = CreateUIObject("RecIndicator", interviewHUD.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-120, -140), new Vector2(18, 18), Color.gray);
            Image recIndicatorImg = recIndicatorObj.GetComponent<Image>();

            Text voiceTimer = CreateText("VoiceTimer", interviewHUD.transform, "00:00 / 01:00", 15, FontStyle.Normal, Color.white, new Vector2(-50, -140), new Vector2(110, 30), defaultFont, TextAnchor.MiddleLeft);

            // Audio Level Meter Bar
            GameObject levelBg = CreateUIObject("AudioLevelBg", interviewHUD.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180, -140), new Vector2(240, 18), new Color(0.2f, 0.2f, 0.25f));
            GameObject levelFill = CreateUIObject("AudioLevelFill", levelBg.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0, 0), new Vector2(240, 18), new Color(0.2f, 0.8f, 0.3f));
            Image levelFillImg = levelFill.GetComponent<Image>();
            levelFillImg.type = Image.Type.Filled;
            levelFillImg.fillMethod = Image.FillMethod.Horizontal;
            levelFillImg.fillAmount = 0f;

            Text voiceStatus = CreateText("VoiceStatus", interviewHUD.transform, "Microphone idle", 13, FontStyle.Italic, Color.gray, new Vector2(180, -165), new Vector2(260, 25), defaultFont);

            // Navigation Buttons
            Button prevBtn = CreateButton("PreviousBtn", interviewHUD.transform, "← Previous Question", new Vector2(-360, -280), new Vector2(220, 50), new Color(0.35f, 0.38f, 0.45f), defaultFont, 16);
            Button nextBtn = CreateButton("NextBtn", interviewHUD.transform, "Next Question →", new Vector2(360, -280), new Vector2(220, 50), new Color(0.2f, 0.55f, 0.85f), defaultFont, 16);
            Button finishBtn = CreateButton("FinishBtn", interviewHUD.transform, "Finish Interview ✔", new Vector2(360, -280), new Vector2(220, 50), new Color(0.15f, 0.70f, 0.35f), defaultFont, 16);
            finishBtn.gameObject.SetActive(false);

            // Link InterviewUI References
            SerializedObject intSo = new SerializedObject(interviewScript);
            intSo.FindProperty("questionCounterText").objectReferenceValue = qCounter;
            intSo.FindProperty("questionCategoryText").objectReferenceValue = qCategory;
            intSo.FindProperty("questionContentText").objectReferenceValue = qContent;
            intSo.FindProperty("questionHintText").objectReferenceValue = qHint;
            intSo.FindProperty("answerInputField").objectReferenceValue = answerInput;
            intSo.FindProperty("wordCountText").objectReferenceValue = wordCount;
            intSo.FindProperty("voiceRecordButton").objectReferenceValue = voiceBtn;
            intSo.FindProperty("voiceRecordButtonText").objectReferenceValue = voiceBtnText;
            intSo.FindProperty("voiceRecordingIndicator").objectReferenceValue = recIndicatorImg;
            intSo.FindProperty("voiceTimerText").objectReferenceValue = voiceTimer;
            intSo.FindProperty("audioLevelBar").objectReferenceValue = levelFillImg;
            intSo.FindProperty("voiceStatusText").objectReferenceValue = voiceStatus;
            intSo.FindProperty("previousButton").objectReferenceValue = prevBtn;
            intSo.FindProperty("nextButton").objectReferenceValue = nextBtn;
            intSo.FindProperty("finishButton").objectReferenceValue = finishBtn;
            intSo.ApplyModifiedProperties();
            interviewHUD.SetActive(false);

            // --- Panel 3: Results Panel ---
            GameObject resultsHUD = CreateUIObject("Results_Panel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 800), new Color(0.08f, 0.10f, 0.14f, 0.95f));
            ResultsUI resultsScript = resultsHUD.AddComponent<ResultsUI>();

            Text resCandidate = CreateText("CandidateHeader", resultsHUD.transform, "Candidate: Student Candidate", 18, FontStyle.Normal, Color.gray, new Vector2(0, 355), new Vector2(700, 30), defaultFont);
            CreateText("OverallLabel", resultsHUD.transform, "OVERALL PERFORMANCE SCORE", 20, FontStyle.Bold, Color.white, new Vector2(0, 320), new Vector2(700, 35), defaultFont);
            Text overallScore = CreateText("OverallScoreVal", resultsHUD.transform, "8.2 / 10.0", 38, FontStyle.Bold, new Color(0.2f, 0.8f, 0.4f), new Vector2(0, 275), new Vector2(400, 50), defaultFont);
            Text recBanner = CreateText("RecBanner", resultsHUD.transform, "COMPETENT PERFORMANCE — RECOMMEND FOR HIRE", 16, FontStyle.Bold, new Color(0.2f, 0.6f, 0.9f), new Vector2(0, 235), new Vector2(800, 30), defaultFont);

            // Metric Meters (Relevance, Clarity, Completeness)
            var (relText, relBar) = CreateMetricRow("Relevance", resultsHUD.transform, new Vector2(-320, 160), defaultFont);
            var (clarText, clarBar) = CreateMetricRow("Clarity", resultsHUD.transform, new Vector2(0, 160), defaultFont);
            var (compText, compBar) = CreateMetricRow("Completeness", resultsHUD.transform, new Vector2(320, 160), defaultFont);

            // Per-Question Review Box
            GameObject reviewBox = CreateUIObject("ReviewBox", resultsHUD.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -60), new Vector2(1000, 340), new Color(0.12f, 0.15f, 0.20f));
            Text revCounter = CreateText("RevCounter", reviewBox.transform, "Reviewing Question 1 of 5 (Score: 8.0/10)", 16, FontStyle.Bold, new Color(0.2f, 0.7f, 1f), new Vector2(0, 140), new Vector2(900, 25), defaultFont);
            Text revQuestion = CreateText("RevQuestion", reviewBox.transform, "Q: Question text here...", 15, FontStyle.Bold, Color.white, new Vector2(0, 105), new Vector2(940, 45), defaultFont, TextAnchor.MiddleLeft);
            Text revAnswer = CreateText("RevAnswer", reviewBox.transform, "Your Answer:\n...", 14, FontStyle.Italic, new Color(0.85f, 0.88f, 0.92f), new Vector2(0, 35), new Vector2(940, 75), defaultFont, TextAnchor.UpperLeft);
            Text revStrengths = CreateText("RevStrengths", reviewBox.transform, "Strengths:\n• Good terminology", 13, FontStyle.Normal, new Color(0.4f, 0.85f, 0.5f), new Vector2(-235, -70), new Vector2(460, 95), defaultFont, TextAnchor.UpperLeft);
            Text revImprovements = CreateText("RevImprovements", reviewBox.transform, "Suggestions for Improvement:\n• Add concrete examples", 13, FontStyle.Normal, new Color(1f, 0.75f, 0.35f), new Vector2(235, -70), new Vector2(460, 95), defaultFont, TextAnchor.UpperLeft);

            Button prevRevBtn = CreateButton("PrevRevBtn", reviewBox.transform, "← Previous Question", new Vector2(-200, -135), new Vector2(180, 35), new Color(0.28f, 0.32f, 0.40f), defaultFont, 13);
            Button nextRevBtn = CreateButton("NextRevBtn", reviewBox.transform, "Next Question →", new Vector2(200, -135), new Vector2(180, 35), new Color(0.28f, 0.32f, 0.40f), defaultFont, 13);

            // Results Actions
            Button returnMenuBtn = CreateButton("ReturnMenuBtn", resultsHUD.transform, "Return to Main Menu", new Vector2(-180, -320), new Vector2(260, 50), new Color(0.2f, 0.55f, 0.85f), defaultFont, 16);
            Button resHistoryBtn = CreateButton("ResHistoryBtn", resultsHUD.transform, "View Session History", new Vector2(180, -320), new Vector2(260, 50), new Color(0.35f, 0.40f, 0.48f), defaultFont, 16);

            // Link ResultsUI References
            SerializedObject resSo = new SerializedObject(resultsScript);
            resSo.FindProperty("candidateNameText").objectReferenceValue = resCandidate;
            resSo.FindProperty("overallScoreText").objectReferenceValue = overallScore;
            resSo.FindProperty("hireRecommendationText").objectReferenceValue = recBanner;
            resSo.FindProperty("relevanceScoreText").objectReferenceValue = relText;
            resSo.FindProperty("relevanceScoreBar").objectReferenceValue = relBar;
            resSo.FindProperty("clarityScoreText").objectReferenceValue = clarText;
            resSo.FindProperty("clarityScoreBar").objectReferenceValue = clarBar;
            resSo.FindProperty("completenessScoreText").objectReferenceValue = compText;
            resSo.FindProperty("completenessScoreBar").objectReferenceValue = compBar;
            resSo.FindProperty("reviewCounterText").objectReferenceValue = revCounter;
            resSo.FindProperty("reviewQuestionText").objectReferenceValue = revQuestion;
            resSo.FindProperty("reviewAnswerText").objectReferenceValue = revAnswer;
            resSo.FindProperty("reviewStrengthsText").objectReferenceValue = revStrengths;
            resSo.FindProperty("reviewImprovementsText").objectReferenceValue = revImprovements;
            resSo.FindProperty("prevReviewButton").objectReferenceValue = prevRevBtn;
            resSo.FindProperty("nextReviewButton").objectReferenceValue = nextRevBtn;
            resSo.FindProperty("returnToMenuButton").objectReferenceValue = returnMenuBtn;
            resSo.FindProperty("viewHistoryButton").objectReferenceValue = resHistoryBtn;
            resSo.ApplyModifiedProperties();
            resultsHUD.SetActive(false);

            // --- Panel 4: History Modal ---
            GameObject histModal = CreateUIObject("History_Modal_Panel", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 600), new Color(0.08f, 0.10f, 0.14f, 0.98f));
            HistoryUI histScript = histModal.AddComponent<HistoryUI>();
            CreateText("HistHeader", histModal.transform, "PAST INTERVIEW SESSIONS (SQLITE)", 22, FontStyle.Bold, Color.white, new Vector2(0, 250), new Vector2(700, 40), defaultFont);
            Text histSummary = CreateText("HistSummaryText", histModal.transform, "Loading history...", 14, FontStyle.Normal, new Color(0.85f, 0.88f, 0.92f), new Vector2(0, 20), new Vector2(750, 380), defaultFont, TextAnchor.UpperLeft);
            Button closeHistBtn = CreateButton("CloseHistBtn", histModal.transform, "Close History", new Vector2(0, -240), new Vector2(200, 45), new Color(0.2f, 0.55f, 0.85f), defaultFont, 16);

            SerializedObject histSo = new SerializedObject(histScript);
            histSo.FindProperty("historyModalPanel").objectReferenceValue = histModal;
            histSo.FindProperty("historySummaryText").objectReferenceValue = histSummary;
            histSo.FindProperty("closeButton").objectReferenceValue = closeHistBtn;
            histSo.ApplyModifiedProperties();
            histModal.SetActive(false);

            // --- Panel 5: Toast & Loading Overlay ---
            GameObject toastObj = CreateUIObject("Toast_Notification_System", canvasObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1080), Color.clear);
            ToastNotification toastScript = toastObj.AddComponent<ToastNotification>();

            // Floating Toast Bubble
            GameObject toastBubble = CreateUIObject("ToastBubble", toastObj.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(750, 60), new Color(0.12f, 0.15f, 0.20f, 0.95f));
            Text bubbleText = CreateText("ToastText", toastBubble.transform, "Notification Message", 16, FontStyle.Normal, Color.white, Vector2.zero, new Vector2(700, 50), defaultFont);
            toastBubble.SetActive(false);

            // Fullscreen Loading Overlay
            GameObject loadOverlay = CreateUIObject("LoadingOverlay", toastObj.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1920, 1080), new Color(0, 0, 0, 0.75f));
            GameObject spinner = CreateUIObject("Spinner", loadOverlay.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(60, 60), new Color(0.2f, 0.6f, 1.0f));
            Text loadText = CreateText("LoadingText", loadOverlay.transform, "Evaluating answers with AI...", 18, FontStyle.Bold, Color.white, new Vector2(0, -30), new Vector2(600, 40), defaultFont);
            loadOverlay.SetActive(false);

            SerializedObject toastSo = new SerializedObject(toastScript);
            toastSo.FindProperty("toastPanel").objectReferenceValue = toastBubble;
            toastSo.FindProperty("toastText").objectReferenceValue = bubbleText;
            toastSo.FindProperty("loadingOverlay").objectReferenceValue = loadOverlay;
            toastSo.FindProperty("loadingMessageText").objectReferenceValue = loadText;
            toastSo.FindProperty("spinnerImage").objectReferenceValue = spinner.GetComponent<Image>();
            toastSo.ApplyModifiedProperties();

            // 5. Create Master AppManager Controller
            GameObject appManager = GameObject.Find("AppManager");
            if (appManager == null)
            {
                appManager = new GameObject("AppManager");
                Undo.RegisterCreatedObjectUndo(appManager, "Create AppManager");
            }

            NetworkClient netClient = appManager.GetComponent<NetworkClient>() ?? appManager.AddComponent<NetworkClient>();
            MicrophoneRecorder micRecorder = appManager.GetComponent<MicrophoneRecorder>() ?? appManager.AddComponent<MicrophoneRecorder>();
            GameManager gm = appManager.GetComponent<GameManager>() ?? appManager.AddComponent<GameManager>();

            SerializedObject gmSo = new SerializedObject(gm);
            gmSo.FindProperty("networkClient").objectReferenceValue = netClient;
            gmSo.FindProperty("microphoneRecorder").objectReferenceValue = micRecorder;
            gmSo.FindProperty("virtualInterviewer").objectReferenceValue = roomGen.interviewerInstance != null ? roomGen.interviewerInstance.GetComponent<VirtualInterviewer>() : Object.FindObjectOfType<VirtualInterviewer>();
            gmSo.FindProperty("mainMenuPanel").objectReferenceValue = menuPanel;
            gmSo.FindProperty("interviewPanel").objectReferenceValue = interviewHUD;
            gmSo.FindProperty("resultsPanel").objectReferenceValue = resultsHUD;
            gmSo.FindProperty("mainMenuUI").objectReferenceValue = menuUI;
            gmSo.FindProperty("interviewUI").objectReferenceValue = interviewScript;
            gmSo.FindProperty("resultsUI").objectReferenceValue = resultsScript;
            gmSo.FindProperty("historyUI").objectReferenceValue = histScript;
            gmSo.FindProperty("toastNotification").objectReferenceValue = toastScript;
            gmSo.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
         Undo.CollapseUndoOperations(undoGroup);

            EditorUtility.DisplayDialog("Scene Setup Complete!",
                "The complete 3D Office Room, Virtual Interviewer, and Canvas UI have been generated and hooked up!\n\n" +
                "1. Start the Flask backend (python app.py)\n" +
                "2. Press Play in the Unity Editor to practice your interview!", "Awesome!");
        }

        // ==============================================================
        // Helper UI Factory Methods
        // ==============================================================

        private static GameObject CreateUIObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            if (color != Color.clear)
            {
                Image img = obj.AddComponent<Image>();
                img.color = color;
            }

            return obj;
        }

        private static Text CreateText(string name, Transform parent, string text, int fontSize, FontStyle style, Color color, Vector2 pos, Vector2 size, Font font, TextAnchor align = TextAnchor.MiddleCenter)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text t = obj.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = color;
            t.font = font;
            t.alignment = align;
            return t;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16)
        {
            GameObject obj = CreateUIObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, bgColor);
            Button btn = obj.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.15f;
            cb.pressedColor = bgColor * 0.85f;
            btn.colors = cb;

            CreateText("Label", obj.transform, label, fontSize, FontStyle.Bold, Color.white, Vector2.zero, size, font);
            return btn;
        }

        private static InputField CreateInputField(string name, Transform parent, Vector2 pos, Vector2 size, string placeholder, Font font, bool isMultiline = false)
        {
            GameObject obj = CreateUIObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, new Color(0.18f, 0.22f, 0.28f));
            InputField input = obj.AddComponent<InputField>();

            Text textComp = CreateText("Text", obj.transform, "", 16, FontStyle.Normal, Color.white, Vector2.zero, size - new Vector2(20, 10), font, isMultiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
            Text placeComp = CreateText("Placeholder", obj.transform, placeholder, 15, FontStyle.Italic, new Color(0.6f, 0.65f, 0.7f), Vector2.zero, size - new Vector2(20, 10), font, isMultiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);

            input.textComponent = textComp;
            input.placeholder = placeComp;
            if (isMultiline)
            {
                input.lineType = InputField.LineType.MultiLineNewline;
            }

            return input;
        }

        private static (Text, Image) CreateMetricRow(string title, Transform parent, Vector2 pos, Font font)
        {
            GameObject container = CreateUIObject(title + "_Metric", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(260, 60), new Color(0.12f, 0.15f, 0.20f));
            CreateText("Title", container.transform, title, 14, FontStyle.Bold, Color.white, new Vector2(0, 15), new Vector2(240, 20), font);

            GameObject barBg = CreateUIObject("BarBg", container.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(240, 14), new Color(0.2f, 0.25f, 0.32f));
            GameObject barFill = CreateUIObject("BarFill", barBg.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(240, 14), new Color(0.2f, 0.75f, 0.35f));

            Image fillImg = barFill.GetComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0.8f;

            Text valText = CreateText("ValText", container.transform, "8.0 / 10.0", 12, FontStyle.Normal, Color.gray, new Vector2(0, -25), new Vector2(240, 16), font);

            return (valText, fillImg);
        }
    }
}
#endif
