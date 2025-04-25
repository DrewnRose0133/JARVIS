using System;
using System.Linq;
using System.Speech.Recognition;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Handles voice recognition and dispatches recognized commands to the CommandRouter,
    /// and raises transcription events for external subscribers.
    /// </summary>
    public class VoiceInput
    {
        private readonly CommandRouter _router;
        private readonly ILogger<VoiceInput> _logger;
        private SpeechRecognitionEngine _recognizer;

        /// <summary>
        /// Raised whenever a transcription is received.
        /// Subscribers can handle the raw recognized text.
        /// </summary>
        public event Action<string> TranscriptionReceived;

        public VoiceInput(CommandRouter router, ILogger<VoiceInput> logger)
        {
            _router = router;
            _logger = logger;
        }

        /// <summary>
        /// Convenience method for subscribing to transcription events.
        /// Mimics an OnTranscriptionReceived handler attachment.
        /// </summary>
        public void OnTranscriptionReceived(Action<string> callback)
        {
            TranscriptionReceived += callback;
        }

        /// <summary>
        /// Starts continuous speech recognition on the default audio device.
        /// </summary>
        public void StartListening()
        {
            // 1) Pick an English recognizer explicitly
            var engines = SpeechRecognitionEngine.InstalledRecognizers();
            var info = engines
                .FirstOrDefault(r => r.Culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                ?? engines.FirstOrDefault();
            if (info == null)
                throw new InvalidOperationException("No speech engines installed!");
            _recognizer = new SpeechRecognitionEngine(info);

            // 2) Load dictation grammar
            _recognizer.LoadGrammar(new DictationGrammar());

            // 3) Wire up both success and rejection so you can debug
            _recognizer.SpeechRecognized += (s, e) =>
            {
                var text = e.Result.Text.ToLower();
                _logger.LogInformation("Recognized: {Text}", text);
                _router.HandleCommand(text);
                TranscriptionReceived?.Invoke(text);
            };
            _recognizer.SpeechRecognitionRejected += (s, e) =>
            {
                _logger.LogWarning("Recognition rejected; audio may have been too quiet or noisy.");
            };

            // 4) Hook up the mic and start async
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);

            _logger.LogInformation("Voice input listening started with engine {0}.", info.Culture.DisplayName);
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
