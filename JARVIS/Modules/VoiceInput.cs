
using System;
using System.Speech.Recognition;

namespace JARVIS.Modules
{
    public static class VoiceInput
    {
        public static string Listen()
        {
            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine())
            {
                recognizer.LoadGrammar(new DictationGrammar());
                recognizer.SetInputToDefaultAudioDevice();
                Console.WriteLine("Listening for command...");
                RecognitionResult result = recognizer.Recognize();
                return result?.Text;
            }
        }
    }
}
