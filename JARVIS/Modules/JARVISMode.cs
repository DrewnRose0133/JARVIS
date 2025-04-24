using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    /// <summary>
    /// Handles Jarvis-specific speech patterns and responses.
    /// </summary>
    public static class JARVISMode
    {
        private static int sarcasmLevel = 0;

        // Populate with your actual intents and phrases
        private static readonly Dictionary<string, string[]> phrases = new Dictionary<string, string[]>
        {
            { "greeting", new[] { "Hello, sir.", "Greetings, sir." } },
            { "farewell", new[] { "Goodbye.", "Until next time." } },
            // ... other intents and their phrase arrays ...
        };

        /// <summary>
        /// Speaks a phrase corresponding to the given intent, using the current sarcasm level.
        /// </summary>
        /// <param name="intent">Key for the phrase set to use.</param>
        public static async Task Speak(string intent)
        {
            if (!phrases.ContainsKey(intent))
            {
                await VoiceOutput.SpeakAsync("Command acknowledged.");
                return;
            }

            int maxIndex = phrases[intent].Length - 1;
            int index = Math.Min(maxIndex, sarcasmLevel / 4);
            await VoiceOutput.SpeakAsync(phrases[intent][index]);
        }

        /// <summary>
        /// Adjusts the sarcasm level (0 = none, increasing levels introduce more sarcastic tiers).
        /// </summary>
        public static void SetSarcasmLevel(int level)
        {
            sarcasmLevel = Math.Max(0, level);
        }
    }
}
