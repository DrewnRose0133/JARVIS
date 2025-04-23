
using System;

namespace JARVIS.Modules
{
    public static class PersonalityEngine
    {
        public enum Personality { Professional, Friendly, Sarcastic, Jarvis }
        public static Personality Current = Personality.Jarvis;

        public static void Speak(string message)
        {
            switch (Current)
            {
                case Personality.Jarvis:
                    JARVISMode.Speak(message);
                    break;
                case Personality.Sarcastic:
                    VoiceOutput.SpeakAsync("Oh, really? " + message);
                    break;
                case Personality.Professional:
                    VoiceOutput.SpeakAsync("System response: " + message);
                    break;
                default:
                    VoiceOutput.SpeakAsync(message);
                    break;
            }
        }

        public static void SetPersonality(string mode)
        {
            if (Enum.TryParse(mode, true, out Personality newMode))
            {
                Current = newMode;
                Logger.Log($"Personality changed to {Current}");
                VoiceOutput.SpeakAsync($"Personality set to {Current}");
            }
            else
            {
                Logger.Log($"Invalid personality: {mode}");
                VoiceOutput.SpeakAsync("That personality isn't available.");
            }
        }
    }
}
