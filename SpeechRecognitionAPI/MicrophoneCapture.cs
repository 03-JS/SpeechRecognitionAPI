using System;
using NAudio.Wave;

namespace SpeechRecognitionAPI;
using Dissonance.Audio.Capture;

public class MicrophoneCapture : IMicrophoneSubscriber
{
    public void ReceiveMicrophoneData(ArraySegment<float> buffer, WaveFormat format)
    {
        Plugin.SpeechEngine.ReceiveAudio(buffer, format.SampleRate);
    }

    public void Reset() { }
}