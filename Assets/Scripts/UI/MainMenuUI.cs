using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRInterviewPractice.UI
{
    /// <summary>
    /// Manages the Main Menu UI: Candidate profile, Interview category selection,
    /// instructions dialog, session history trigger, and application exit.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Input & Selections")]
        [SerializeField] private InputField candidateNameInput;
        [SerializeField] private Button hrCategoryButton;
        [SerializeField] private Button technicalCategoryButton;
        [SerializeField] private Text selectedCategoryLabel;

        [Header("Primary Action Buttons")]
        [SerializeField] private Button startInterviewButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button exitButton;

        [Header("Instructions Modal")]
        [SerializeField] private GameObject instructionsModal;
        [SerializeField] private Button closeInstructionsButton;

        [Header("Status Indicator")]
        [SerializeField] private Text backendStatusText;

        // Current Selected Category ("hr" or "technical")
        private string selectedCategory = "hr";

        // Events to GameManager
        public event Action<string, string> OnStartInterviewRequested; // (category, candidateName)
        public event Action OnViewHistoryRequested;

        private void Start()
        {
            SetupListeners();
            SelectCategory("hr");
        }

        private void SetupListeners()
        {
            if (hrCategoryButton != null)
                hrCategoryButton.onClick.AddListener(() => SelectCategory("hr"));

            if (technicalCategoryButton != null)
                technicalCategoryButton.onClick.AddListener(() => SelectCategory("technical"));

            if (startInterviewButton != null)
                startInterviewButton.onClick.AddListener(HandleStartClick);

            if (instructionsButton != null)
                instructionsButton.onClick.AddListener(() => ShowInstructions(true));

            if (closeInstructionsButton != null)
                closeInstructionsButton.onClick.AddListener(() => ShowInstructions(false));

            if (historyButton != null)
                historyButton.onClick.AddListener(() => OnViewHistoryRequested?.Invoke());

            if (exitButton != null)
                exitButton.onClick.AddListener(HandleExit);
        }

        public void SelectCategory(string category)
        {
            selectedCategory = category.ToLower();
            if (selectedCategoryLabel != null)
            {
                selectedCategoryLabel.text = selectedCategory == "hr"
                    ? "Selected: Behavioral & HR Interview (5 Questions)"
                    : "Selected: Technical Systems & Coding Interview (5 Questions)";
            }

            // Visual highlight styling on buttons
            if (hrCategoryButton != null)
            {
                var colors = hrCategoryButton.colors;
                colors.normalColor = (selectedCategory == "hr") ? new Color(0.2f, 0.6f, 0.9f) : Color.white;
                hrCategoryButton.colors = colors;
            }

            if (technicalCategoryButton != null)
            {
                var colors = technicalCategoryButton.colors;
                colors.normalColor = (selectedCategory == "technical") ? new Color(0.2f, 0.6f, 0.9f) : Color.white;
                technicalCategoryButton.colors = colors;
            }
        }

        private void HandleStartClick()
        {
            string candidateName = candidateNameInput != null && !string.IsNullOrWhiteSpace(candidateNameInput.text)
                ? candidateNameInput.text.Trim()
                : "Student Candidate";

            OnStartInterviewRequested?.Invoke(selectedCategory, candidateName);
        }

        public void ShowInstructions(bool show)
        {
            if (instructionsModal != null)
            {
                instructionsModal.SetActive(show);
            }
        }

        public void UpdateBackendStatus(string status, bool isOnline)
        {
            if (backendStatusText != null)
            {
                backendStatusText.text = status;
                backendStatusText.color = isOnline ? new Color(0.1f, 0.75f, 0.2f) : new Color(0.9f, 0.25f, 0.2f);
            }
        }

        private void HandleExit()
        {
            Debug.Log("[MainMenuUI] Exiting Interview Practice System...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
