using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using VRInterviewPractice.Models;

namespace VRInterviewPractice.Core
{
    /// <summary>
    /// Handles all asynchronous HTTP REST networking between the Unity client
    /// and the Python Flask backend using UnityWebRequest.
    /// </summary>
    public class NetworkClient : MonoBehaviour
    {
        [Header("Backend Connection")]
        [SerializeField] private string backendBaseUrl = "http://127.0.0.1:5000";
        [SerializeField] private int timeoutSeconds = 15;

        public string BackendBaseUrl
        {
            get => backendBaseUrl;
            set => backendBaseUrl = value.TrimEnd('/');
        }

        private void Awake()
        {
            backendBaseUrl = backendBaseUrl.TrimEnd('/');
        }

        // ==============================================================
        // 1. Health Check
        // ==============================================================

        public void CheckHealth(Action<bool, string> callback)
        {
            StartCoroutine(GetRoutine("/health", (success, text) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, "Cannot connect to backend. Please ensure 'python app.py' is running on port 5000.");
                    return;
                }

                try
                {
                    HealthResponse res = JsonUtility.FromJson<HealthResponse>(text);
                    callback?.Invoke(true, $"Backend status: {res.status} (AI Mode: {res.ai_provider})");
                }
                catch (Exception ex)
                {
                    callback?.Invoke(false, $"Health parsing error: {ex.Message}");
                }
            }));
        }

        // ==============================================================
        // 2. Fetch Interview Questions
        // ==============================================================

        public void FetchQuestions(string category, Action<bool, List<QuestionItem>, string> callback)
        {
            string url = $"/api/questions?category={UnityWebRequest.EscapeURL(category)}";
            StartCoroutine(GetRoutine(url, (success, text) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, null, "Failed to load questions from backend. Please verify server connection.");
                    return;
                }

                try
                {
                    QuestionsResponse res = JsonUtility.FromJson<QuestionsResponse>(text);
                    if (res != null && res.success && res.questions != null)
                    {
                        callback?.Invoke(true, res.questions, null);
                    }
                    else
                    {
                        callback?.Invoke(false, null, "Received invalid questions payload from server.");
                    }
                }
                catch (Exception ex)
                {
                    callback?.Invoke(false, null, $"JSON parse error: {ex.Message}");
                }
            }));
        }

        // ==============================================================
        // 3. Audio Transcription (WAV Upload)
        // ==============================================================

        public void TranscribeAudio(byte[] wavBytes, Action<bool, string, string> callback)
        {
            if (wavBytes == null || wavBytes.Length == 0)
            {
                callback?.Invoke(false, null, "Cannot transcribe empty audio buffer.");
                return;
            }

            StartCoroutine(UploadAudioRoutine(wavBytes, callback));
        }

        private IEnumerator UploadAudioRoutine(byte[] wavBytes, Action<bool, string, string> callback)
        {
            string url = $"{backendBaseUrl}/api/transcribe";

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("audio", wavBytes, "user_recording.wav", "audio/wav")
            };

            using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
            {
                www.timeout = timeoutSeconds;
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isNetworkError = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetworkError = www.isNetworkError || www.isHttpError;
#endif

                if (isNetworkError)
                {
                    string err = string.IsNullOrEmpty(www.error) ? "Speech transcription failed." : www.error;
                    callback?.Invoke(false, null, err);
                }
                else
                {
                    try
                    {
                        string json = www.downloadHandler.text;
                        TranscriptionResponse res = JsonUtility.FromJson<TranscriptionResponse>(json);
                        if (res != null && res.success)
                        {
                            callback?.Invoke(true, res.transcript, null);
                        }
                        else
                        {
                            callback?.Invoke(false, null, res?.error ?? "Transcription failed.");
                        }
                    }
                    catch (Exception ex)
                    {
                        callback?.Invoke(false, null, $"Audio response parse error: {ex.Message}");
                    }
                }
            }
        }

        // ==============================================================
        // 4. Answer Evaluation
        // ==============================================================

        public void EvaluateAnswer(string question, string answer, string category, Action<bool, EvaluationResponse, string> callback)
        {
            EvaluationRequest req = new EvaluationRequest
            {
                question = question,
                answer = answer,
                category = category
            };

            string jsonBody = JsonUtility.ToJson(req);

            StartCoroutine(PostJsonRoutine("/api/evaluate", jsonBody, (success, responseText) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, null, "Failed to evaluate answer. Please verify backend connection.");
                    return;
                }

                try
                {
                    EvaluationResponse res = JsonUtility.FromJson<EvaluationResponse>(responseText);
                    callback?.Invoke(true, res, null);
                }
                catch (Exception ex)
                {
                    callback?.Invoke(false, null, $"Evaluation parse error: {ex.Message}");
                }
            }));
        }

        // ==============================================================
        // 5. Save Finished Interview Session
        // ==============================================================

        public void SaveSession(SaveSessionRequest req, Action<bool, string, string> callback)
        {
            string jsonBody = JsonUtility.ToJson(req);

            StartCoroutine(PostJsonRoutine("/api/save_session", jsonBody, (success, responseText) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, null, "Failed to save session to database.");
                    return;
                }

                try
                {
                    SaveSessionResponse res = JsonUtility.FromJson<SaveSessionResponse>(responseText);
                    if (res != null && res.success)
                    {
                        callback?.Invoke(true, res.session_id, null);
                    }
                    else
                    {
                        callback?.Invoke(false, null, res?.error ?? "Save session returned failure.");
                    }
                }
                catch (Exception ex)
                {
                    callback?.Invoke(false, null, $"Save parse error: {ex.Message}");
                }
            }));
        }

        // ==============================================================
        // 6. Fetch Past Interview History
        // ==============================================================

        public void FetchHistory(Action<bool, List<SessionHistorySummary>, string> callback)
        {
            StartCoroutine(GetRoutine("/api/history", (success, text) =>
            {
                if (!success)
                {
                    callback?.Invoke(false, null, "Failed to load past session history.");
                    return;
                }

                try
                {
                    HistoryResponse res = JsonUtility.FromJson<HistoryResponse>(text);
                    if (res != null && res.success && res.sessions != null)
                    {
                        callback?.Invoke(true, res.sessions, null);
                    }
                    else
                    {
                        callback?.Invoke(false, null, "Failed to parse history records.");
                    }
                }
                catch (Exception ex)
                {
                    callback?.Invoke(false, null, $"History JSON parse error: {ex.Message}");
                }
            }));
        }

        // ==============================================================
        // Low-level HTTP Helper Routines
        // ==============================================================

        private IEnumerator GetRoutine(string endpoint, Action<bool, string> onComplete)
        {
            string fullUrl = $"{backendBaseUrl}{endpoint}";
            using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
            {
                www.timeout = timeoutSeconds;
                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool error = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
#else
                bool error = www.isNetworkError || www.isHttpError;
#endif

                if (error)
                {
                    Debug.LogWarning($"[NetworkClient] GET error at {fullUrl}: {www.error}");
                    onComplete?.Invoke(false, www.error);
                }
                else
                {
                    onComplete?.Invoke(true, www.downloadHandler.text);
                }
            }
        }

        private IEnumerator PostJsonRoutine(string endpoint, string jsonBody, Action<bool, string> onComplete)
        {
            string fullUrl = $"{backendBaseUrl}{endpoint}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest www = new UnityWebRequest(fullUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = timeoutSeconds;

                yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool error = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
#else
                bool error = www.isNetworkError || www.isHttpError;
#endif

                if (error)
                {
                    Debug.LogWarning($"[NetworkClient] POST error at {fullUrl}: {www.error}");
                    onComplete?.Invoke(false, www.error);
                }
                else
                {
                    onComplete?.Invoke(true, www.downloadHandler.text);
                }
            }
        }
    }
}
