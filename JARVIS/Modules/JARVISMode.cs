
using System;
using System.Collections.Generic;

namespace JARVIS.Modules
{
    public static class JARVISMode
    {
        private static int sarcasmLevel = 5;

        private static Dictionary<string, string[]> phrases = new Dictionary<string, string[]>
        {
            ["garage_open"] = new[] {
                "The garage is now open, sir.",
                "Garage access granted. Try not to drive through the wall.",
                "Once again, the garage is at your service. I'm thrilled."
            },
            ["lights_on"] = new[] {
                "Illumination achieved.",
                "I've turned on the lights. Revolutionary, really.",
                "Lights on. Try not to waste electricity."
            },
            ["ac_set"] = new[] {
                "Temperature adjusted.",
                "Climate controls set. You’re welcome.",
                "Air conditioning adjusted. Perhaps a blanket too?"
            }
        };

        public static void SetSarcasm(int level)
        {
            sarcasmLevel = Math.Clamp(level, 0, 10);
            Logger.Log($"Sarcasm level set to {sarcasmLevel}");
        }

        public static void Speak(string intent)
        {
            if (!phrases.ContainsKey(intent))
            {
                VoiceOutput.SpeakAsync("Command acknowledged.");
                return;
            }

            int index = Math.Min(phrases[intent].Length - 1, sarcasmLevel / 4);
            VoiceOutput.SpeakAsync(phrases[intent][index]);
        }
    }
}
