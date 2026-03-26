using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using SpeechRecognitionAPI.Util;
using UnityEngine;
using Vosk;

namespace SpeechRecognitionAPI
{
    public class Engine : MonoBehaviour
    {
        public static event EventHandler<SpeechEventArgs> SpeechRecognized;
        private string[] models = { "vosk-model-small-en-us-0.15", "vosk-model-small-es-0.42" };

        private VoskRecognizer _recognizer;
        private Model _model;
        private MicrophoneCapture _micCapture;
        private int _micSampleRate;

        // Starts the Speech Recognition model and does some set up
        public void StartEngine()
        {
            DontDestroyOnLoad(this);
            string modelPath = Path.Combine(
                Plugin.pluginDir, "models", models[(int)Plugin.language.Value]
            );
            _model = new Model(modelPath);
            _recognizer = new VoskRecognizer(_model, 16000f);
            Plugin.mls.LogInfo("Vosk ASR ready.");
        }

        public void StartMicCapture()
        {
            StartCoroutine(WaitForDissonance());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator WaitForDissonance()
        {
            var dissonance = FindObjectOfType<Dissonance.DissonanceComms>();
            while (!dissonance || dissonance.MicrophoneCapture == null)
            {
                dissonance = FindObjectOfType<Dissonance.DissonanceComms>();
                yield return new WaitForSeconds(0.5f);
            }

            _micCapture = new MicrophoneCapture();
            dissonance.MicrophoneCapture.Subscribe(_micCapture);
            Plugin.mls.LogInfo("Subscribed to Dissonance microphone stream");
        }

        public void StopMicCapture()
        {
            var dissonance = FindObjectOfType<Dissonance.DissonanceComms>();
            if (dissonance == null)
            {
                Plugin.mls.LogError("DissonanceComms not found");
                return;
            }

            dissonance.MicrophoneCapture.Unsubscribe(_micCapture);
        }

        private float[] Resample(float[] input, int inputRate, int outputRate)
        {
            if (inputRate == outputRate) return input;
            int outputLength = (int)((long)input.Length * outputRate / inputRate);
            float[] output = new float[outputLength];
            float ratio = (float)input.Length / outputLength;
            for (int i = 0; i < outputLength; i++)
            {
                float srcIndex = i * ratio;
                int index = (int)srcIndex;
                float frac = srcIndex - index;
                float a = input[index];
                float b = index + 1 < input.Length ? input[index + 1] : a;
                output[i] = a + (b - a) * frac;
            }

            return output;
        }

        public void ReceiveAudio(ArraySegment<float> buffer, int sampleRate)
        {
            float[] samples = new float[buffer.Count];
            if (buffer.Array == null) return;
            Array.Copy(buffer.Array, buffer.Offset, samples, 0, buffer.Count);

            // Resample to 16000 Hz for Vosk
            float[] resampled = Resample(samples, sampleRate, 16000);

            byte[] pcm = new byte[resampled.Length * 2];
            for (int i = 0; i < resampled.Length; i++)
            {
                short s = (short)(resampled[i] * 32767);
                pcm[i * 2] = (byte)(s & 0xff);
                pcm[i * 2 + 1] = (byte)(s >> 8);
            }

            if (_recognizer.AcceptWaveform(pcm, pcm.Length))
            {
                // This triggers when a silence/pause is detected (Final Result)
                string json = _recognizer.Result();
                var resultObj = JsonConvert.DeserializeObject<VoskResult>(json);
                string? recognized = resultObj?.text;
                if (!string.IsNullOrEmpty(recognized))
                {
                    if (Plugin.logging.Value) Plugin.mls.LogInfo($"Recognized: {recognized}");
                    if (Speech.phrases.Count > 0) Speech.GetBestMatch(recognized);
                    SpeechRecognized?.Invoke(this, new SpeechEventArgs(recognized));
                }
            }
        }
    }
}