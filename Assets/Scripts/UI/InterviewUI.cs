using System;
using UnityEngine;
using UnityEngine.UI;
using VRInterviewPractice.Models;

namespace VRInterviewPractice.UI
{
    /// <summary>
    /// Controls the active interview HUD: question display, typed answer area,
    /// microphone recording status, volume metering, navigation, and finish actions.
    /// </summary>
    public class InterviewUI : MonoBehaviour
    {
        [Header("Question Header & Text")]
        [SerializeField] private Text questionCounterText;
        [SerializeField] private Text questionCategoryText;
        [SerializeField] private Text questionContentText;
        [SerializeField] private Text questionHintText;

        [Header("Typed Answer Area")]
        [SerializeField] private InputField answerInputField;
        [SerializeField] private Text wordCountText;

        [Header("Voice Recording Controls")]
        [SerializeField] private Button voiceRecordButton;
        [SerializeField] private Text voiceRecordButtonText;
        [SerializeField] private Image voiceRecordingIndicator;
        [SerializeField] private Text voiceTimerText;
        [SerializeField] private Image audioLevelBar;
        [SerializeField] private Text voiceStatusText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button finishButton;

        // Active State
        private int currentQuestionId = -1;
        private string currentInputMode = "text"; // "text" or "voice"
        private bool isRecording = false;

        // Events to GameManager
        public event Action OnToggleVoiceRecording;
        public event Action<int, string, string> OnSaveCurrentAnswer; // (questionId, answerText, inputMode)
        public event Action OnPreviousQuestion;
        public event Action OnNextQuestion;
        public event Action OnFinishInterview;

        private void Start()
        {
            SetupListeners();
        }

        private void SetupListeners()
        {
            if (voiceRecordButton != null)
                voiceRecordButton.onClick.AddListener(() => OnToggleVoiceRecording?.Invoke());

            if (previousButton != null)
                previousButton.onClick.AddListener(HandlePreviousClick);

            if (nextButton != null)
                nextButton.onClick.AddListener(HandleNextClick);

            if (finishButton != null)
                finishButton.onClick.AddListener(HandleFinishClick);

            if (answerInputField != null)
                answerInputField.onValueChanged.AddListener(HandleAnswerTextChanged);
        }

        public void DisplayQuestion(QuestionItem question, int currentIndex, int totalQuestions, string savedAnswer = "")
        {
            if (question == null) return;

            currentQuestionId = question.id;
            currentInputMode = "text";

            if (questionCounterText != null)
                questionCounterText.text = $"Question {currentIndex + 1} of {totalQuestions}";

            if (questionCategoryText != null)
                questionCategoryText.text = $"{question.category.ToUpper()} INTERVIEW";

            if (questionContentText != null)
                questionContentText.text = question.question;

            if (questionHintText != null)
                questionHintText.text = !string.IsNullOrEmpty(question.guidelines) ? $"Hint: {question.guidelines}" : "";

            if (answerInputField != null)
            {
                answerInputField.text = savedAnswer ?? "";
                UpdateWordCount(answerInputField.text);
            }

            // Update navigation button states
            if (previousButton != null)
                previousButton.interactable = (currentIndex > 0);

            bool isLast = (currentIndex >= totalQuestions - 1);
            if (nextButton != null)
                nextButton.gameObject.SetActive(!isLast);

            if (finishButton != null)
                finishButton.gameObject.SetActive(isLast);

            ResetVoiceUI();
        }

        public string GetCurrentAnswerText()
        {
            return answerInputField != null ? answerInputField.text.Trim() : "";
        }

        public void SetAnswerFromTranscription(string transcribedText)
        {
            currentInputMode = "voice";
            if (answerInputField != null)
            {
                if (string.IsNullOrWhiteSpace(answerInputField.text))
                {
                    answerInputField.text = transcribedText;
                }
                else
                {
                    answerInputField.text += " " + transcribedText;
                }
                UpdateWordCount(answerInputField.text);
            }

            if (voiceStatusText != null)
            {
                voiceStatusText.text = "Voice transcribed successfully!";
                voiceStatusText.color = new Color(0.1f, 0.7f, 0.2f);
            }
        }

        public void SetRecordingState(bool recording)
        {
            isRecording = recording;

            if (voiceRecordButtonText != null)
                voiceRecordButtonText.text = isRecording ? "Stop Recording" : "Record Voice Answer";

            if (voiceRecordingIndicator != null)
                voiceRecordingIndicator.color = isRecording ? Color.red : Color.gray;

            if (voiceStatusText != null)
            {
                voiceStatusText.text = isRecording ? "Recording... Speak clearly into your mic" : "Microphone idle";
                voiceStatusText.color = isRecording ? new Color(0.9f, 0.3f, 0.2f) : Color.gray;
            }
        }

        public void UpdateRecordingTimer(float elapsedSeconds)
        {
            if (voiceTimerText != null)
            {
                int min = Mathf.FloorToInt(elapsedSeconds / 60f);
                int sec = Mathf.FloorToInt(elapsedSeconds % 60f);
                voiceTimerText.text = $"{min:00}:{sec:00} / 01:00";
            }
        }

        public void UpdateAudioLevel(float level)
        {
            if (audioLevelBar != null)
            {
                audioLevelBar.fillAmount = Mathf.Clamp01(level);
            }
        }

        private void ResetVoiceUI()
        {
            SetRecordingState(false);
            UpdateRecordingTimer(0f);
            UpdateAudioLevel(0f);
        }

        private void HandleAnswerTextChanged(string newText)
        {
            UpdateWordCount(newText);
        }

        private void UpdateWordCount(string text)
        {
            if (wordCountText != null)
            {
                string[] words = (text ?? "").Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                wordCountText.text = $"Word Count: {words.Length}";
            }
        }

        private void HandlePreviousClick()
        {
            SaveCurrentState();
            OnPreviousQuestion?.Invoke();
        }

        private void HandleNextClick()
        {
            SaveCurrentState();
            OnNextQuestion?.Invoke();
        }

        private void HandleFinishClick()
        {
            SaveCurrentState();
            OnFinishInterview?.Invoke();
        }

        private void SaveCurrentState()
        {
            if (currentQuestionId > 0)
            {
                string text = GetCurrentAnswerText();
                OnSaveCurrentAnswer?.Invoke(currentQuestionId, text, currentInputMode);
            }
        }
    }
}
