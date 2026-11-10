using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRInterviewPractice.Audio;
using VRInterviewPractice.Environment;
using VRInterviewPractice.Models;
using VRInterviewPractice.UI;

namespace VRInterviewPractice.Core
{
    public enum AppState
    {
        MainMenu,
        Interviewing,
        Results,
        History
    }

    /// <summary>
    /// Master state machine coordinator connecting networking, microphone capture,
    /// 3D interviewer reactions, and UI panels.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Subsystems")]
        [SerializeField] private NetworkClient networkClient;
        [SerializeField] private MicrophoneRecorder microphoneRecorder;
        [SerializeField] private VirtualInterviewer virtualInterviewer;

        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject interviewPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private InterviewUI interviewUI;
        [SerializeField] private ResultsUI resultsUI;
        [SerializeField] private HistoryUI historyUI;
        [SerializeField] private ToastNotification toastNotification;

        // Current Session
        private InterviewSession currentSession;
        private AppState currentState = AppState.MainMenu;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            InitializeSubsystems();
            SetupEventSubscriptions();
            CheckBackendConnection();
            SetState(AppState.MainMenu);
        }

        private void InitializeSubsystems()
        {
            if (networkClient == null) networkClient = FindObjectOfType<NetworkClient>();
            if (microphoneRecorder == null) microphoneRecorder = FindObjectOfType<MicrophoneRecorder>();
            if (virtualInterviewer == null) virtualInterviewer = FindObjectOfType<VirtualInterviewer>();
            if (toastNotification == null) toastNotification = FindObjectOfType<ToastNotification>();
        }

        private void SetupEventSubscriptions()
        {
            // Main Menu Events
            if (mainMenuUI != null)
            {
                mainMenuUI.OnStartInterviewRequested += StartInterviewFlow;
                mainMenuUI.OnViewHistoryRequested += ShowHistoryModal;
            }

            // Interview UI Events
            if (interviewUI != null)
            {
                interviewUI.OnToggleVoiceRecording += HandleToggleVoiceRecording;
                interviewUI.OnSaveCurrentAnswer += HandleSaveCurrentAnswer;
                interviewUI.OnPreviousQuestion += HandleNavigatePrevious;
                interviewUI.OnNextQuestion += HandleNavigateNext;
                interviewUI.OnFinishInterview += HandleFinishInterview;
            }

            // Results UI Events
            if (resultsUI != null)
            {
                resultsUI.OnReturnToMenuRequested += () => SetState(AppState.MainMenu);
                resultsUI.OnViewHistoryRequested += ShowHistoryModal;
            }

            // Microphone Events
            if (microphoneRecorder != null)
            {
                microphoneRecorder.OnRecordingStarted += () =>
                {
                    if (interviewUI != null) interviewUI.SetRecordingState(true);
                };

                microphoneRecorder.OnRecordingFinished += (wavBytes, durationSec) =>
                {
                    if (interviewUI != null) interviewUI.SetRecordingState(false);
                    ProcessVoiceTranscription(wavBytes);
                };

                microphoneRecorder.OnAudioLevelUpdated += (level) =>
                {
                    if (interviewUI != null) interviewUI.UpdateAudioLevel(level);
                };

                microphoneRecorder.OnRecordingError += (errorMsg) =>
                {
                    if (interviewUI != null) interviewUI.SetRecordingState(false);
                    if (toastNotification != null) toastNotification.ShowToast(errorMsg, 4.0f);
                };
            }
        }

        private void Update()
        {
            // Update recording elapsed timer in HUD
            if (currentState == AppState.Interviewing && microphoneRecorder != null && microphoneRecorder.IsRecording)
            {
                if (interviewUI != null)
                {
                    interviewUI.UpdateRecordingTimer(microphoneRecorder.RecordingElapsed);
                }
            }
        }

        private void CheckBackendConnection()
        {
            if (networkClient == null) return;

            networkClient.CheckHealth((isOnline, message) =>
            {
                if (mainMenuUI != null)
                {
                    mainMenuUI.UpdateBackendStatus(isOnline ? "Backend: Online" : "Backend: Offline (Start Flask)", isOnline);
                }
                if (!isOnline && toastNotification != null)
                {
                    toastNotification.ShowToast("Flask backend is offline. Run 'python app.py' in the backend folder.", 5.0f);
                }
            });
        }

        public void SetState(AppState newState)
        {
            currentState = newState;

            if (mainMenuPanel != null) mainMenuPanel.SetActive(newState == AppState.MainMenu);
            if (interviewPanel != null) interviewPanel.SetActive(newState == AppState.Interviewing);
            if (resultsPanel != null) resultsPanel.SetActive(newState == AppState.Results);
        }

        // ==============================================================
        // Interview Flow Actions
        // ==============================================================

        private void StartInterviewFlow(string category, string candidateName)
        {
            if (toastNotification != null) toastNotification.ShowLoading("Loading interview questions...");

            networkClient.FetchQuestions(category, (success, questions, error) =>
            {
                if (toastNotification != null) toastNotification.HideLoading();

                if (!success || questions == null || questions.Count == 0)
                {
                    if (toastNotification != null)
                        toastNotification.ShowToast(error ?? "Could not load questions. Check Flask backend.", 4f);
                    return;
                }

                currentSession = new InterviewSession(category, candidateName);
                currentSession.SetQuestions(questions);

                SetState(AppState.Interviewing);
                DisplayCurrentQuestionInHUD();

                if (toastNotification != null)
                    toastNotification.ShowToast($"Interview started: {questions.Count} questions.", 2.5f);
            });
        }

        private void DisplayCurrentQuestionInHUD()
        {
            if (currentSession == null || interviewUI == null) return;

            QuestionItem q = currentSession.CurrentQuestion;
            if (q == null) return;

            var existingResp = currentSession.GetResponseForQuestion(q.id);
            string savedAnswer = existingResp != null ? existingResp.answer_text : "";

            interviewUI.DisplayQuestion(q, currentSession.CurrentIndex, currentSession.TotalQuestions, savedAnswer);
        }

        private void HandleSaveCurrentAnswer(int questionId, string answerText, string inputMode)
        {
            if (currentSession == null || currentSession.CurrentQuestion == null) return;
            currentSession.SaveAnswer(questionId, currentSession.CurrentQuestion.question, answerText, inputMode, null);
        }

        private void HandleNavigatePrevious()
        {
            if (currentSession != null && currentSession.MovePrevious())
            {
                DisplayCurrentQuestionInHUD();
            }
        }

        private void HandleNavigateNext()
        {
            if (currentSession == null) return;

            // Prompt if answer is completely empty
            string currentAnswer = interviewUI != null ? interviewUI.GetCurrentAnswerText() : "";
            if (string.IsNullOrWhiteSpace(currentAnswer))
            {
                if (toastNotification != null)
                    toastNotification.ShowToast("Tip: Answering before moving forward ensures a comprehensive final score.", 3.0f);
            }

            if (currentSession.MoveNext())
            {
                DisplayCurrentQuestionInHUD();
            }
        }

        private void HandleToggleVoiceRecording()
        {
            if (microphoneRecorder == null) return;

            if (microphoneRecorder.IsRecording)
            {
                microphoneRecorder.StopRecording();
            }
            else
            {
                microphoneRecorder.StartRecording();
            }
        }

        private void ProcessVoiceTranscription(byte[] wavBytes)
        {
            if (toastNotification != null) toastNotification.ShowLoading("Transcribing voice answer...");

            networkClient.TranscribeAudio(wavBytes, (success, transcript, error) =>
            {
                if (toastNotification != null) toastNotification.HideLoading();

                if (success && !string.IsNullOrEmpty(transcript))
                {
                    if (interviewUI != null) interviewUI.SetAnswerFromTranscription(transcript);
                    if (virtualInterviewer != null) virtualInterviewer.PlayAffirmativeNod();
                }
                else
                {
                    if (toastNotification != null)
                        toastNotification.ShowToast(error ?? "Speech transcription failed. Please use typed answer fallback.", 4f);
                }
            });
        }

        private void HandleFinishInterview()
        {
            if (currentSession == null) return;

            // Save the last active question answer
            if (interviewUI != null && currentSession.CurrentQuestion != null)
            {
                string text = interviewUI.GetCurrentAnswerText();
                currentSession.SaveAnswer(currentSession.CurrentQuestion.id, currentSession.CurrentQuestion.question, text, "text", null);
            }

            StartCoroutine(EvaluateAndFinalizeSessionRoutine());
        }

        private IEnumerator EvaluateAndFinalizeSessionRoutine()
        {
            if (toastNotification != null)
                toastNotification.ShowLoading("AI is evaluating your answers and generating feedback report...");

            List<QuestionItem> allQuestions = currentSession.Questions;
            int total = allQuestions.Count;

            for (int i = 0; i < total; i++)
            {
                QuestionItem q = allQuestions[i];
                var savedRec = currentSession.GetResponseForQuestion(q.id);
                string answer = savedRec != null ? savedRec.answer_text : "";

                bool evalDone = false;
                EvaluationResponse receivedEval = null;

                networkClient.EvaluateAnswer(q.question, answer, currentSession.Category, (success, eval, err) =>
                {
                    receivedEval = eval;
                    evalDone = true;
                });

                while (!evalDone) yield return null;

                // Save evaluation into session
                currentSession.SaveAnswer(q.id, q.question, answer, savedRec?.input_mode ?? "text", receivedEval);
            }

            // Save session to SQLite database
            bool saveDone = false;
            SaveSessionRequest saveReq = currentSession.ToSaveRequest();

            networkClient.SaveSession(saveReq, (success, sessionId, err) =>
            {
                saveDone = true;
            });

            while (!saveDone) yield return null;

            if (toastNotification != null) toastNotification.HideLoading();

            if (virtualInterviewer != null) virtualInterviewer.PlayAffirmativeNod();

            // Display Results Screen
            SetState(AppState.Results);
            if (resultsUI != null)
            {
                resultsUI.DisplayResults(
                    currentSession.CandidateName,
                    currentSession.CalculateOverallAverageScore(),
                    currentSession.GetAllResponses()
                );
            }
        }

        private void ShowHistoryModal()
        {
            if (toastNotification != null) toastNotification.ShowLoading("Loading interview history from SQLite...");

            networkClient.FetchHistory((success, sessions, err) =>
            {
                if (toastNotification != null) toastNotification.HideLoading();

                if (success && historyUI != null)
                {
                    historyUI.DisplayHistory(sessions);
                }
                else
                {
                    if (toastNotification != null)
                        toastNotification.ShowToast(err ?? "Failed to retrieve history.", 3f);
                }
            });
        }
    }
}
