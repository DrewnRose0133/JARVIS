
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
                    VoiceOutput.Speak("Oh, really? " + message);
                    break;
                case Personality.Professional:
                    VoiceOutput.Speak("System response: " + message);
                    break;
                default:
                    VoiceOutput.Speak(message);
                    break;
            }
        }

        public static void SetPersonality(string mode)
        {
            if (Enum.TryParse(mode, true, out Personality newMode))
            {
                Current = newMode;
                Logger.Log($"Personality changed to {Current}");
                VoiceOutput.Speak($"Personality set to {Current}");
            }
            else
            {
                Logger.Log($"Invalid personality: {mode}");
                VoiceOutput.Speak("That personality isn't available.");
            }
        }
    }
}
