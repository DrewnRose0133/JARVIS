using System;
using System.Speech.Recognition;

namespace JARVIS.Modules
{
    public static class VoiceInput
    {
        private static SpeechRecognitionEngine recognizer;

        public static void StartListening()
        {
            recognizer = new SpeechRecognitionEngine();
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(new DictationGrammar());

            recognizer.SpeechRecognized += (s, e) =>
            {
                string input = e.Result.Text.ToLower();
                Logger.Log($"Voice recognized: {input}");
                CommandRouter.HandleCommand(input);
            };

            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            Logger.Log("Voice input listening started.");
        }

        public static void StopListening()
        {
            recognizer?.RecognizeAsyncStop();
        }
    }
}