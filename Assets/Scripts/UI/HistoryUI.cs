using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRInterviewPractice.Models;

namespace VRInterviewPractice.UI
{
    /// <summary>
    /// Displays past interview session records fetched from the SQLite database.
    /// </summary>
    public class HistoryUI : MonoBehaviour
    {
        [Header("Modal Components")]
        [SerializeField] private GameObject historyModalPanel;
        [SerializeField] private Text historySummaryText;
        [SerializeField] private Button closeButton;

        public event Action OnCloseRequested;

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void DisplayHistory(List<SessionHistorySummary> sessions)
        {
            if (historyModalPanel != null) historyModalPanel.SetActive(true);

            if (historySummaryText == null) return;

            if (sessions == null || sessions.Count == 0)
            {
                historySummaryText.text = "No previous interview sessions found in database.\nComplete your first practice session to see records here!";
                return;
            }

            string report = "PAST PRACTICE SESSIONS (SQLITE):\n\n";
            int idx = 1;
            foreach (var s in sessions)
            {
                string dateStr = s.created_at;
                if (DateTime.TryParse(s.created_at, out DateTime dt))
                {
                    dateStr = dt.ToString("yyyy-MM-dd HH:mm");
                }

                report += $"#{idx++} | Candidate: {s.candidate_name}\n" +
                          $"     Category: {s.category.ToUpper()} | Score: {s.overall_score:F1}/10.0\n" +
                          $"     Questions: {s.total_questions} | Date: {dateStr}\n" +
                          "--------------------------------------------------\n";
            }

            historySummaryText.text = report;
        }

        public void Close()
        {
            if (historyModalPanel != null) historyModalPanel.SetActive(false);
            OnCloseRequested?.Invoke();
        }
    }
}
