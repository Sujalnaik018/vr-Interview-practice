using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRInterviewPractice.Models;

namespace VRInterviewPractice.UI
{
    /// <summary>
    /// Displays comprehensive post-interview results, AI scoring breakdown,
    /// qualitative strengths and improvements, and question review navigation.
    /// </summary>
    public class ResultsUI : MonoBehaviour
    {
        [Header("Header Summary")]
        [SerializeField] private Text candidateNameText;
        [SerializeField] private Text overallScoreText;
        [SerializeField] private Text hireRecommendationText;

        [Header("Category Metrics")]
        [SerializeField] private Text relevanceScoreText;
        [SerializeField] private Image relevanceScoreBar;
        [SerializeField] private Text clarityScoreText;
        [SerializeField] private Image clarityScoreBar;
        [SerializeField] private Text completenessScoreText;
        [SerializeField] private Image completenessScoreBar;

        [Header("Per-Question Feedback Review")]
        [SerializeField] private Text reviewCounterText;
        [SerializeField] private Text reviewQuestionText;
        [SerializeField] private Text reviewAnswerText;
        [SerializeField] private Text reviewStrengthsText;
        [SerializeField] private Text reviewImprovementsText;
        [SerializeField] private Button prevReviewButton;
        [SerializeField] private Button nextReviewButton;

        [Header("Actions")]
        [SerializeField] private Button viewHistoryButton;
        [SerializeField] private Button returnToMenuButton;

        // Active State
        private List<QuestionResponseRecord> evaluatedResponses = new List<QuestionResponseRecord>();
        private int currentReviewIndex = 0;

        // Events
        public event Action OnReturnToMenuRequested;
        public event Action OnViewHistoryRequested;

        private void Start()
        {
            SetupListeners();
        }

        private void SetupListeners()
        {
            if (prevReviewButton != null)
                prevReviewButton.onClick.AddListener(ShowPreviousQuestionReview);

            if (nextReviewButton != null)
                nextReviewButton.onClick.AddListener(ShowNextQuestionReview);

            if (viewHistoryButton != null)
                viewHistoryButton.onClick.AddListener(() => OnViewHistoryRequested?.Invoke());

            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(() => OnReturnToMenuRequested?.Invoke());
        }

        public void DisplayResults(string candidateName, float overallScore, List<QuestionResponseRecord> responses)
        {
            evaluatedResponses = responses ?? new List<QuestionResponseRecord>();
            currentReviewIndex = 0;

            if (candidateNameText != null)
                candidateNameText.text = $"Candidate: {candidateName}";

            if (overallScoreText != null)
                overallScoreText.text = $"{overallScore:F1} / 10.0";

            if (hireRecommendationText != null)
            {
                if (overallScore >= 8.5f)
                {
                    hireRecommendationText.text = "EXCEPTIONAL PERFORMANCE — HIGH RECOMMENDATION";
                    hireRecommendationText.color = new Color(0.1f, 0.75f, 0.2f);
                }
                else if (overallScore >= 7.0f)
                {
                    hireRecommendationText.text = "COMPETENT PERFORMANCE — RECOMMEND FOR HIRE";
                    hireRecommendationText.color = new Color(0.2f, 0.6f, 0.9f);
                }
                else if (overallScore >= 5.0f)
                {
                    hireRecommendationText.text = "SATISFACTORY — NEEDS FURTHER TARGETED PRACTICE";
                    hireRecommendationText.color = new Color(0.9f, 0.6f, 0.1f);
                }
                else
                {
                    hireRecommendationText.text = "FOUNDATIONAL — REQUIRES CONCEPTUAL STRENGTHENING";
                    hireRecommendationText.color = new Color(0.85f, 0.2f, 0.2f);
                }
            }

            // Calculate Metric Averages
            float avgRelevance = 0f, avgClarity = 0f, avgCompleteness = 0f;
            if (evaluatedResponses.Count > 0)
            {
                foreach (var r in evaluatedResponses)
                {
                    avgRelevance += r.relevance_score;
                    avgClarity += r.clarity_score;
                    avgCompleteness += r.completeness_score;
                }
                avgRelevance /= evaluatedResponses.Count;
                avgClarity /= evaluatedResponses.Count;
                avgCompleteness /= evaluatedResponses.Count;
            }

            // Update Progress Bars & Text
            SetMetric(relevanceScoreText, relevanceScoreBar, avgRelevance);
            SetMetric(clarityScoreText, clarityScoreBar, avgClarity);
            SetMetric(completenessScoreText, completenessScoreBar, avgCompleteness);

            DisplayCurrentQuestionReview();
        }

        private void SetMetric(Text label, Image bar, float score)
        {
            if (label != null) label.text = $"{score:F1} / 10.0";
            if (bar != null) bar.fillAmount = Mathf.Clamp01(score / 10f);
        }

        private void DisplayCurrentQuestionReview()
        {
            if (evaluatedResponses == null || evaluatedResponses.Count == 0)
            {
                if (reviewQuestionText != null) reviewQuestionText.text = "No questions available to review.";
                return;
            }

            QuestionResponseRecord rec = evaluatedResponses[currentReviewIndex];

            if (reviewCounterText != null)
                reviewCounterText.text = $"Reviewing Question {currentReviewIndex + 1} of {evaluatedResponses.Count} (Score: {rec.overall_score:F1}/10)";

            if (reviewQuestionText != null)
                reviewQuestionText.text = $"Q: {rec.question_text}";

            if (reviewAnswerText != null)
                reviewAnswerText.text = $"Your Answer ({rec.input_mode}):\n\"{rec.answer_text}\"";

            if (reviewStrengthsText != null)
            {
                string s = "Strengths:\n";
                if (rec.feedback_strengths != null && rec.feedback_strengths.Count > 0)
                {
                    foreach (var item in rec.feedback_strengths) s += $"• {item}\n";
                }
                else
                {
                    s += "• Direct and clear response.";
                }
                reviewStrengthsText.text = s.TrimEnd();
            }

            if (reviewImprovementsText != null)
            {
                string imp = "Suggestions for Improvement:\n";
                if (rec.feedback_improvements != null && rec.feedback_improvements.Count > 0)
                {
                    foreach (var item in rec.feedback_improvements) imp += $"• {item}\n";
                }
                else
                {
                    imp += "• Continue to refine your pacing and detail.";
                }
                reviewImprovementsText.text = imp.TrimEnd();
            }

            if (prevReviewButton != null)
                prevReviewButton.interactable = (currentReviewIndex > 0);

            if (nextReviewButton != null)
                nextReviewButton.interactable = (currentReviewIndex < evaluatedResponses.Count - 1);
        }

        private void ShowPreviousQuestionReview()
        {
            if (currentReviewIndex > 0)
            {
                currentReviewIndex--;
                DisplayCurrentQuestionReview();
            }
        }

        private void ShowNextQuestionReview()
        {
            if (currentReviewIndex < evaluatedResponses.Count - 1)
            {
                currentReviewIndex++;
                DisplayCurrentQuestionReview();
            }
        }
    }
}
