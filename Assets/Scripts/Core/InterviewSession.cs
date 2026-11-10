using System;
using System.Collections.Generic;
using UnityEngine;
using VRInterviewPractice.Models;

namespace VRInterviewPractice.Core
{
    /// <summary>
    /// Encapsulates the runtime state of a single candidate interview session.
    /// </summary>
    public class InterviewSession
    {
        public string SessionId { get; private set; }
        public string CandidateName { get; set; } = "Student Candidate";
        public string Category { get; set; } = "hr";

        private List<QuestionItem> questions = new List<QuestionItem>();
        private Dictionary<int, QuestionResponseRecord> responses = new Dictionary<int, QuestionResponseRecord>();
        private int currentQuestionIndex = 0;

        public List<QuestionItem> Questions => questions;
        public int CurrentIndex => currentQuestionIndex;
        public int TotalQuestions => questions != null ? questions.Count : 0;
        public int CurrentQuestionNumber => currentQuestionIndex + 1;

        public bool HasNext => currentQuestionIndex < TotalQuestions - 1;
        public bool HasPrevious => currentQuestionIndex > 0;

        public QuestionItem CurrentQuestion =>
            (questions != null && currentQuestionIndex >= 0 && currentQuestionIndex < questions.Count)
                ? questions[currentQuestionIndex]
                : null;

        public InterviewSession(string category, string candidateName = "Student Candidate")
        {
            SessionId = Guid.NewGuid().ToString();
            Category = category;
            CandidateName = string.IsNullOrEmpty(candidateName) ? "Student Candidate" : candidateName;
            questions = new List<QuestionItem>();
            responses = new Dictionary<int, QuestionResponseRecord>();
            currentQuestionIndex = 0;
        }

        public void SetQuestions(List<QuestionItem> newQuestions)
        {
            questions = newQuestions ?? new List<QuestionItem>();
            currentQuestionIndex = 0;
            responses.Clear();
        }

        public bool MoveNext()
        {
            if (HasNext)
            {
                currentQuestionIndex++;
                return true;
            }
            return false;
        }

        public bool MovePrevious()
        {
            if (HasPrevious)
            {
                currentQuestionIndex--;
                return true;
            }
            return false;
        }

        public void SaveAnswer(int questionId, string questionText, string answerText, string inputMode, EvaluationResponse evaluation = null)
        {
            QuestionResponseRecord record = new QuestionResponseRecord
            {
                question_id = questionId,
                question_text = questionText,
                answer_text = answerText,
                input_mode = inputMode,
                relevance_score = evaluation != null ? evaluation.relevance_score : 0f,
                clarity_score = evaluation != null ? evaluation.clarity_score : 0f,
                completeness_score = evaluation != null ? evaluation.completeness_score : 0f,
                overall_score = evaluation != null ? evaluation.overall_score : 0f,
                feedback_strengths = evaluation?.strengths ?? new List<string>(),
                feedback_improvements = evaluation?.improvements ?? new List<string>()
            };

            responses[questionId] = record;
        }

        public QuestionResponseRecord GetResponseForQuestion(int questionId)
        {
            if (responses.TryGetValue(questionId, out QuestionResponseRecord rec))
            {
                return rec;
            }
            return null;
        }

        public List<QuestionResponseRecord> GetAllResponses()
        {
            return new List<QuestionResponseRecord>(responses.Values);
        }

        public float CalculateOverallAverageScore()
        {
            if (responses.Count == 0) return 0f;

            float sum = 0f;
            foreach (var r in responses.Values)
            {
                sum += r.overall_score;
            }
            return (float)Math.Round(sum / responses.Count, 1);
        }

        public SaveSessionRequest ToSaveRequest()
        {
            return new SaveSessionRequest
            {
                session_id = SessionId,
                candidate_name = CandidateName,
                category = Category,
                overall_score = CalculateOverallAverageScore(),
                responses = GetAllResponses()
            };
        }
    }
}
