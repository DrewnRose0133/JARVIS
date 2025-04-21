
using System.Speech.Synthesis;

namespace JARVIS.Modules
{
    public static class VoiceOutput
    {
        private static SpeechSynthesizer synth = new SpeechSynthesizer();

        public static void Speak(string text)
        {
            synth.Speak(text);
        }
    }
}
