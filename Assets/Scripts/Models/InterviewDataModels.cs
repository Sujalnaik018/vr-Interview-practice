using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRInterviewPractice.Models
{
    // ==============================================================
    // Questions Bank Models
    // ==============================================================

    [Serializable]
    public class QuestionItem
    {
        public int id;
        public string category;
        public string question;
        public string guidelines;
    }

    [Serializable]
    public class QuestionsResponse
    {
        public bool success;
        public string category;
        public int count;
        public List<QuestionItem> questions;
    }

    // ==============================================================
    // Answer Evaluation Models
    // ==============================================================

    [Serializable]
    public class EvaluationRequest
    {
        public string question;
        public string answer;
        public string category;
    }

    [Serializable]
    public class EvaluationResponse
    {
        public bool success;
        public float relevance_score;
        public float clarity_score;
        public float completeness_score;
        public float overall_score;
        public List<string> strengths;
        public List<string> improvements;
        public string mode;
        public string notice;
    }

    // ==============================================================
    // Audio Transcription Models
    // ==============================================================

    [Serializable]
    public class TranscriptionResponse
    {
        public bool success;
        public string transcript;
        public float confidence;
        public float duration_sec;
        public string provider;
        public string error;
    }

    // ==============================================================
    // Session Persistence & History Models
    // ==============================================================

    [Serializable]
    public class QuestionResponseRecord
    {
        public int question_id;
        public string question_text;
        public string answer_text;
        public string input_mode; // "voice" or "text"
        public float relevance_score;
        public float clarity_score;
        public float completeness_score;
        public float overall_score;
        public List<string> feedback_strengths;
        public List<string> feedback_improvements;
    }

    [Serializable]
    public class SaveSessionRequest
    {
        public string session_id;
        public string candidate_name;
        public string category;
        public float overall_score;
        public List<QuestionResponseRecord> responses;
    }

    [Serializable]
    public class SaveSessionResponse
    {
        public bool success;
        public string session_id;
        public int saved_questions;
        public string message;
        public string error;
    }

    [Serializable]
    public class SessionHistorySummary
    {
        public string id;
        public string candidate_name;
        public string category;
        public int total_questions;
        public float overall_score;
        public string created_at;
    }

    [Serializable]
    public class HistoryResponse
    {
        public bool success;
        public int count;
        public List<SessionHistorySummary> sessions;
    }

    [Serializable]
    public class HealthResponse
    {
        public string status;
        public string database;
        public string ai_provider;
        public string timestamp;
    }
}
