using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SpeechRecognitionAPI.Patches;

namespace SpeechRecognitionAPI
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "JS03.SpeechRecognitionAPI";
        private const string modName = "SpeechRecognitionAPI";
        private const string modVersion = "1.0.0";

        public static Engine SpeechEngine;
        private static readonly string[] _libraries = ["libgcc_s_seh-1", "libstdc++-6", "libvosk", "libwinpthread-1", "Vosk"];

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        internal static string? pluginDir;
        private readonly Harmony harmony = new(modGUID);
        public static Plugin Instance;
        internal static ManualLogSource mls;

        // Config
        public static ConfigEntry<bool> logging;
        public static ConfigEntry<Languages> language;

        void Awake()
        {
            if (Instance == null) Instance = this;

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Starting SpeechRecognitionAPI");
            
            language = Config.Bind(
                "General", // Config section
                "Language", // Key of this config
                Languages.English, // Default value
                "Language to be used for speech recognition" // Description
            );

            logging = Config.Bind(
                "General", // Config section
                "Log recognized speech", // Key of this config
                true, // Default value
                "Shows the speech recognition output" // Description
            );
            
            Speech.phrases = new List<string>();

            pluginDir = Path.GetDirectoryName(Info.Location);
            // Prepend plugin dir so libraries are found
            Environment.SetEnvironmentVariable("PATH", pluginDir + ";" + Environment.GetEnvironmentVariable("PATH"));
            
            // Force-load libraries
            foreach (var library in _libraries)
            {
                string libPath = Path.Combine(pluginDir, $"{library}.dll");
                if (File.Exists(libPath))
                    LoadLibrary(libPath);
                else
                    mls.LogError($"{library}.dll not found at: {libPath}");
            }

            harmony.PatchAll(typeof(GameNetworkManagerPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
        }
    }
}