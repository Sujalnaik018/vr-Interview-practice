using System;
using System.Collections;
using UnityEngine;

namespace VRInterviewPractice.Audio
{
    /// <summary>
    /// Manages microphone hardware input, recording state, audio level metering,
    /// and conversion of audio clips to WAV byte arrays.
    /// </summary>
    public class MicrophoneRecorder : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private int sampleRate = 16000;
        [SerializeField] private int maxRecordingDuration = 60; // Max 60 seconds per answer

        [Header("State (Read-Only)")]
        [SerializeField] private bool isRecording = false;
        [SerializeField] private float currentAudioLevel = 0f;
        [SerializeField] private float recordingElapsed = 0f;

        private AudioClip recordingClip;
        private string activeMicrophoneDevice = null;
        private Coroutine recordingTimerCoroutine;

        // Public Callbacks
        public event Action OnRecordingStarted;
        public event Action<byte[], float> OnRecordingFinished; // (wavBytes, durationSec)
        public event Action<string> OnRecordingError;
        public event Action<float> OnAudioLevelUpdated;

        public bool IsRecording => isRecording;
        public float RecordingElapsed => recordingElapsed;

        private void Start()
        {
            CheckMicrophoneAvailability();
        }

        public bool CheckMicrophoneAvailability()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogWarning("[MicrophoneRecorder] No microphone input devices found on system.");
                return false;
            }

            activeMicrophoneDevice = Microphone.devices[0];
            Debug.Log($"[MicrophoneRecorder] Selected Microphone: {activeMicrophoneDevice}");
            return true;
        }

        public void StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("[MicrophoneRecorder] Recording is already active.");
                return;
            }

            if (!CheckMicrophoneAvailability())
            {
                OnRecordingError?.Invoke("No active microphone hardware found. Please plug in a microphone or use typed answers.");
                return;
            }

            try
            {
                recordingElapsed = 0f;
                // Start recording: loop=false, maxDuration=maxRecordingDuration, frequency=sampleRate
                recordingClip = Microphone.Start(activeMicrophoneDevice, false, maxRecordingDuration, sampleRate);
                isRecording = true;

                OnRecordingStarted?.Invoke();
                recordingTimerCoroutine = StartCoroutine(RecordingTimerRoutine());
            }
            catch (Exception ex)
            {
                isRecording = false;
                Debug.LogError($"[MicrophoneRecorder] Failed to start microphone: {ex.Message}");
                OnRecordingError?.Invoke($"Microphone error: {ex.Message}");
            }
        }

        public void StopRecording()
        {
            if (!isRecording) return;

            int recordedSamples = Microphone.GetPosition(activeMicrophoneDevice);
            Microphone.End(activeMicrophoneDevice);
            isRecording = false;

            if (recordingTimerCoroutine != null)
            {
                StopCoroutine(recordingTimerCoroutine);
                recordingTimerCoroutine = null;
            }

            currentAudioLevel = 0f;
            OnAudioLevelUpdated?.Invoke(0f);

            if (recordedSamples <= 0)
            {
                OnRecordingError?.Invoke("Audio recording was empty. Please speak clearly into the microphone.");
                return;
            }

            // Convert to standard WAV byte array
            byte[] wavBytes = WavUtility.FromAudioClip(recordingClip, recordedSamples);
            float actualDuration = (float)recordedSamples / sampleRate;

            OnRecordingFinished?.Invoke(wavBytes, actualDuration);
        }

        private IEnumerator RecordingTimerRoutine()
        {
            float[] sampleBuffer = new float[128];

            while (isRecording)
            {
                recordingElapsed += Time.deltaTime;

                // Compute real-time RMS audio level for UI indicator
                int micPos = Microphone.GetPosition(activeMicrophoneDevice);
                if (micPos > 128 && recordingClip != null)
                {
                    recordingClip.GetData(sampleBuffer, micPos - 128);
                    float sum = 0f;
                    for (int i = 0; i < sampleBuffer.Length; i++)
                    {
                        sum += sampleBuffer[i] * sampleBuffer[i];
                    }
                    currentAudioLevel = Mathf.Clamp01(Mathf.Sqrt(sum / sampleBuffer.Length) * 10f);
                    OnAudioLevelUpdated?.Invoke(currentAudioLevel);
                }

                if (recordingElapsed >= maxRecordingDuration)
                {
                    StopRecording();
                    yield break;
                }

                yield return null;
            }
        }

        private void OnDisable()
        {
            if (isRecording && activeMicrophoneDevice != null)
            {
                Microphone.End(activeMicrophoneDevice);
                isRecording = false;
            }
        }
    }
}
