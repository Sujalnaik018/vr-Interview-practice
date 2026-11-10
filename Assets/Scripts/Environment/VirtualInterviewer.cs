using System.Collections;
using UnityEngine;

namespace VRInterviewPractice.Environment
{
    /// <summary>
    /// Controls natural lifelike motions of the 3D virtual interviewer:
    /// idle breathing, attentive listening, and nodding when candidate completes an answer.
    /// </summary>
    public class VirtualInterviewer : MonoBehaviour
    {
        [Header("Rig Transforms")]
        public Transform headTransform;
        public Transform torsoTransform;

        [Header("Idle Animation Settings")]
        [SerializeField] private float breathSpeed = 1.6f;
        [SerializeField] private float breathHeight = 0.012f;
        [SerializeField] private float headTiltRange = 2.5f;

        private Vector3 initialTorsoPos;
        private Vector3 initialHeadPos;
        private Quaternion initialHeadRot;
        private bool isNodding = false;

        private void Start()
        {
            if (torsoTransform != null) initialTorsoPos = torsoTransform.localPosition;
            if (headTransform != null)
            {
                initialHeadPos = headTransform.localPosition;
                initialHeadRot = headTransform.localRotation;
            }
        }

        private void Update()
        {
            AnimateIdleBreathing();
        }

        private void AnimateIdleBreathing()
        {
            float sine = Mathf.Sin(Time.time * breathSpeed);

            // Subtle vertical chest expansion/contraction
            if (torsoTransform != null)
            {
                torsoTransform.localPosition = initialTorsoPos + new Vector3(0, sine * breathHeight, 0);
            }

            // Subtle head stabilization with slight inquisitive micro-tilt
            if (headTransform != null && !isNodding)
            {
                float microTilt = Mathf.Sin(Time.time * (breathSpeed * 0.5f)) * headTiltRange;
                headTransform.localRotation = initialHeadRot * Quaternion.Euler(sine * 1.0f, 0, microTilt);
            }
        }

        /// <summary>
        /// Triggers an attentive affirmative head nod (e.g. after candidate finishes an answer).
        /// </summary>
        public void PlayAffirmativeNod()
        {
            if (!isNodding && gameObject.activeInHierarchy)
            {
                StartCoroutine(NodRoutine());
            }
        }

        private IEnumerator NodRoutine()
        {
            isNodding = true;
            float duration = 1.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Double nod using damped sine wave
                float pitch = Mathf.Sin((elapsed / duration) * Mathf.PI * 4f) * 8f;
                if (headTransform != null)
                {
                    headTransform.localRotation = initialHeadRot * Quaternion.Euler(pitch, 0, 0);
                }
                yield return null;
            }

            if (headTransform != null)
            {
                headTransform.localRotation = initialHeadRot;
            }
            isNodding = false;
        }
    }
}
