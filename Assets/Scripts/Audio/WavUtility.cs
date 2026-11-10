using System;
using System.IO;
using UnityEngine;

namespace VRInterviewPractice.Audio
{
    /// <summary>
    /// Utility class to encode Unity AudioClip recordings into 16-bit PCM WAV byte arrays.
    /// Compatible with Python wave, Whisper, and SpeechRecognition services.
    /// </summary>
    public static class WavUtility
    {
        private const int HEADER_SIZE = 44;

        public static byte[] FromAudioClip(AudioClip clip, int recordedSamples = -1)
        {
            if (clip == null)
            {
                Debug.LogError("[WavUtility] AudioClip is null!");
                return null;
            }

            int channels = clip.channels;
            int frequency = clip.frequency;

            float[] samples = new float[clip.samples * channels];
            clip.GetData(samples, 0);

            // If a specific number of recorded samples is passed, trim the array
            int totalSamplesToEncode = (recordedSamples > 0 && recordedSamples <= samples.Length)
                ? recordedSamples * channels
                : samples.Length;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    WriteWavHeader(writer, channels, frequency, totalSamplesToEncode);

                    // Convert float samples [-1.0f, 1.0f] to 16-bit signed PCM [-32768, 32767]
                    for (int i = 0; i < totalSamplesToEncode; i++)
                    {
                        float clampedSample = Mathf.Clamp(samples[i], -1.0f, 1.0f);
                        short pcmShort = (short)(clampedSample * 32767f);
                        writer.Write(pcmShort);
                    }
                }
                return stream.ToArray();
            }
        }

        private static void WriteWavHeader(BinaryWriter writer, int channels, int sampleRate, int totalSamples)
        {
            int subChunk2Size = totalSamples * 2; // 2 bytes per 16-bit sample
            int chunkSize = 36 + subChunk2Size;
            short bitsPerSample = 16;
            short audioFormat = 1; // PCM
            int byteRate = sampleRate * channels * (bitsPerSample / 8);
            short blockAlign = (short)(channels * (bitsPerSample / 8));

            // RIFF chunk descriptor
            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(chunkSize);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

            // "fmt " sub-chunk
            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16); // SubChunk1Size (16 for PCM)
            writer.Write(audioFormat);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // "data" sub-chunk
            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(subChunk2Size);
        }
    }
}
