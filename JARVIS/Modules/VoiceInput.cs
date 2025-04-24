using System;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Handles voice recognition and dispatches recognized commands to the CommandRouter.
    /// </summary>
    public class VoiceInput
    {
        private readonly CommandRouter _router;
        private readonly ILogger<VoiceInput> _logger;
        private SpeechRecognitionEngine _recognizer;

        public VoiceInput(CommandRouter router, ILogger<VoiceInput> logger)
        {
            _router = router;
            _logger = logger;
        }

        /// <summary>
        /// Starts continuous speech recognition on the default audio device.
        /// </summary>
        public void StartListening()
        {
            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SpeechRecognized += (s, e) =>
            {
                string input = e.Result.Text.ToLower();
                _logger.LogInformation("Voice recognized: {Input}", input);
                _router.HandleCommand(input);
            };
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            _logger.LogInformation("Voice input listening started.");
        }

        /// <summary>
        /// Stops the ongoing speech recognition.
        /// </summary>
        public void StopListening()
        {
            _recognizer?.RecognizeAsyncStop();
        }
    }
}
