using HarmonyLib;
using UnityEngine;

namespace SpeechRecognitionAPI.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    public class StartOfRoundPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartMicCapture()
        {
            Plugin.SpeechEngine.StartMicCapture();
        }
    }
}