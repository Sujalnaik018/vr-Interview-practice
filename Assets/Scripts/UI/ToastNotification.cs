using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRInterviewPractice.UI
{
    /// <summary>
    /// Displays non-intrusive floating toast notifications and loading overlays.
    /// Uses standard UnityEngine.UI elements for zero-dependency compatibility.
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        [Header("Toast Components")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private Text toastText;

        [Header("Loading Overlay Components")]
        [SerializeField] private GameObject loadingOverlay;
        [SerializeField] private Text loadingMessageText;
        [SerializeField] private Image spinnerImage;

        private Coroutine hideToastCoroutine;

        private void Awake()
        {
            if (toastPanel != null) toastPanel.SetActive(false);
            if (loadingOverlay != null) loadingOverlay.SetActive(false);
        }

        private void Update()
        {
            if (loadingOverlay != null && loadingOverlay.activeSelf && spinnerImage != null)
            {
                spinnerImage.transform.Rotate(0, 0, -200f * Time.deltaTime);
            }
        }

        public void ShowToast(string message, float duration = 3.5f)
        {
            if (toastPanel == null || toastText == null) return;

            toastText.text = message;
            toastPanel.SetActive(true);

            if (hideToastCoroutine != null) StopCoroutine(hideToastCoroutine);
            hideToastCoroutine = StartCoroutine(HideToastRoutine(duration));
        }

        private IEnumerator HideToastRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (toastPanel != null) toastPanel.SetActive(false);
        }

        public void ShowLoading(string message = "Please wait...")
        {
            if (loadingOverlay == null) return;
            if (loadingMessageText != null) loadingMessageText.text = message;
            loadingOverlay.SetActive(true);
        }

        public void HideLoading()
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.SetActive(false);
            }
        }
    }
}
