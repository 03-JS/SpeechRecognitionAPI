using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpeechRecognitionAPI
{
    public class Speech
    {
        internal static List<string> phrases;
        public static string bestMatch;
        private static double bestScore;

        internal static float GetSimilarity(string phrase, string recognized)
        {
            if (string.IsNullOrEmpty(phrase) || string.IsNullOrEmpty(recognized))
                return 0;

            int maxLength = Math.Max(phrase.Length, recognized.Length);
            if (maxLength == 0) return 1.0f;

            int distance = LevenshteinDistance(phrase, recognized);
            float similarity = (float)Math.Round(1.0 - (double)distance / maxLength, 2);
            return similarity;
        }

        internal static void GetBestMatch(string recognized)
        {
            float maxScore = float.MinValue;

            foreach (string phrase in phrases)
            {
                float score = GetSimilarity(phrase, recognized);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMatch = phrase;
                }
            }

            bestScore = maxScore;
            if (Plugin.logging.Value)
            {
                Plugin.mls.LogDebug($"Best match: {bestMatch}");
                Plugin.mls.LogDebug($"Best similarity score: {bestScore}");
            }
        }

        /// <summary>
        /// Determines whether the provided phrases contain the best match and meet the similarity threshold.
        /// </summary>
        /// <param name="phrases">An array of phrases to be checked for similarity.</param>
        /// <param name="similarityThreshold">The minimum similarity score required for a match to be considered valid.</param>
        /// <returns>
        /// True if the phrases array contains the best match and the best score is greater than or equal to the similarity threshold; otherwise, false.
        /// </returns>
        public static bool IsAboveThreshold(string[] phrases, double similarityThreshold)
        {
            return phrases.Contains(bestMatch) && bestScore >= similarityThreshold;
        }

        /// <summary>
        /// Registers a set of phrases.
        /// </summary>
        /// <param name="phrases">The collection of phrases to be registered.</param>
        /// <returns></returns>
        public static void RegisterPhrases(string[] phrases)
        {
            Speech.phrases.AddRange(phrases);
        }

        /// <summary>
        /// Registers a custom event handler for the SpeechRecognized event.
        /// </summary>
        /// <param name="callback">The event handler to be executed when speech is recognized.</param>
        /// <returns>The registered event handler.</returns>
        public static EventHandler<SpeechEventArgs> RegisterCustomHandler(EventHandler<SpeechEventArgs> callback)
        {
            Engine.SpeechRecognized += callback;
            return callback;
        }

        private static int LevenshteinDistance(string s1, string s2)
        {
            int[] prev = new int[s2.Length + 1];
            int[] curr = new int[s2.Length + 1];

            for (int j = 0; j <= s2.Length; j++) prev[j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    curr[j] = Math.Min(
                        Math.Min(prev[j] + 1, curr[j - 1] + 1),
                        prev[j - 1] + cost
                    );
                }
                (prev, curr) = (curr, prev);
            }
            return prev[s2.Length];
        }

        // internal static readonly string[] languages =
        // {
        //     "",
        //     "en",
        //     "zh",
        //     "de",
        //     "es",
        //     "ru",
        //     "ko",
        //     "fr",
        //     "ja",
        //     "pt",
        //     "tr",
        //     "pl",
        //     "ca",
        //     "nl",
        //     "ar",
        //     "sv",
        //     "it",
        //     "id",
        //     "hi",
        //     "fi",
        //     "vi",
        //     "he",
        //     "uk",
        //     "el",
        //     "ms",
        //     "cs",
        //     "ro",
        //     "da",
        //     "hu",
        //     "ta",
        //     "no",
        //     "th",
        //     "ur",
        //     "hr",
        //     "bg",
        //     "lt",
        //     "la",
        //     "mi",
        //     "ml",
        //     "cy",
        //     "sk",
        //     "te",
        //     "fa",
        //     "lv",
        //     "bn",
        //     "sr",
        //     "az",
        //     "sl",
        //     "kn",
        //     "et",
        //     "mk",
        //     "br",
        //     "eu",
        //     "is",
        //     "hy",
        //     "ne",
        //     "mn",
        //     "bs",
        //     "kk",
        //     "sq",
        //     "sw",
        //     "gl",
        //     "mr",
        //     "pa",
        //     "si",
        //     "km",
        //     "sn",
        //     "yo",
        //     "so",
        //     "af",
        //     "oc",
        //     "ka",
        //     "be",
        //     "tg",
        //     "sd",
        //     "gu",
        //     "am",
        //     "yi",
        //     "lo",
        //     "uz",
        //     "fo",
        //     "ht",
        //     "ps",
        //     "tk",
        //     "nn",
        //     "mt",
        //     "sa",
        //     "lb",
        //     "my",
        //     "bo",
        //     "tl",
        //     "mg",
        //     "as",
        //     "tt",
        //     "haw",
        //     "ln",
        //     "ha",
        //     "ba",
        //     "jw",
        //     "su",
        //     "yue"
        // };
    }

    public class SpeechEventArgs : EventArgs
    {
        public string Text { get; }

        public SpeechEventArgs(string text)
        {
            Text = text;
        }
    }

    internal class VoskResult
    {
        public string text;
    }
}
