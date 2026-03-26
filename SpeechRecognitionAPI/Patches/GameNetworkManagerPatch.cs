using HarmonyLib;
using UnityEngine;

namespace SpeechRecognitionAPI.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartModel()
        {
            if (Plugin.pluginDir == null)
            {
                Debug.LogError("[SpeechRecognitionAPI] Plugin did not initialize correctly, skipping engine start.");
                return;
            }
            var speechEngineObject = new GameObject("SpeechRecognitionAPIEngine");
            Plugin.SpeechEngine = speechEngineObject.AddComponent<Engine>();
            Plugin.SpeechEngine.StartEngine();
        }
        
        [HarmonyPatch("Disconnect")]
        [HarmonyPostfix]
        static void StopMicCapture()
        {
            Plugin.SpeechEngine.StopMicCapture();
        }
    }
}
