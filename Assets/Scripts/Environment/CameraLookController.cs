using UnityEngine;

namespace VRInterviewPractice.Environment
{
    /// <summary>
    /// Provides smooth, natural desktop seated head rotation for the candidate camera.
    /// Constrained to realistic neck swivel angles (-45 to +45 deg yaw, -20 to +25 deg pitch).
    /// </summary>
    public class CameraLookController : MonoBehaviour
    {
        [Header("Sensitivity & Limits")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 25f;
        [SerializeField] private float minYaw = -45f;
        [SerializeField] private float maxYaw = 45f;
        [SerializeField] private bool requireRightClickToLook = true;

        private float currentYaw = 0f;
        private float currentPitch = 0f;
        private Quaternion baseRotation;

        private void Start()
        {
            baseRotation = transform.localRotation;
        }

        private void Update()
        {
            // Optional: only look around when right mouse button is held, allowing easy UI interaction
            if (requireRightClickToLook && !Input.GetMouseButton(1))
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            currentYaw = Mathf.Clamp(currentYaw + mouseX, minYaw, maxYaw);
            currentPitch = Mathf.Clamp(currentPitch - mouseY, minPitch, maxPitch);

            transform.localRotation = baseRotation * Quaternion.Euler(currentPitch, currentYaw, 0f);
        }

        public void ResetView()
        {
            currentYaw = 0f;
            currentPitch = 0f;
            transform.localRotation = baseRotation;
        }
    }
}
