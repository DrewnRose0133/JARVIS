using System;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Listens for the "jarvis" wake word and routes commands accordingly,
    /// and raises a WakeWordDetected event for external subscribers.
    /// </summary>
    public class WakeWordListener
    {
        private readonly CommandRouter _router;
        private readonly ILogger<WakeWordListener> _logger;
        private bool _listening;

        /// <summary>
        /// Raised whenever the wake word is detected.
        /// Subscribers can handle the event to trigger additional logic.
        /// </summary>
        public event Action WakeWordDetected;

        public WakeWordListener(CommandRouter router, ILogger<WakeWordListener> logger)
        {
            _router = router;
            _logger = logger;
        }

        /// <summary>
        /// Convenience method for subscribing to wake-word detection events.
        /// Mimics an OnWakeWordDetected handler attachment.
        /// </summary>
        public void OnWakeWordDetected(Action callback)
        {
            WakeWordDetected += callback;
        }

        /// <summary>
        /// Alias to start listening for the wake word.
        /// </summary>
        public void StartListening()
        {
            _listening = true;
            Listen();
        }

        /// <summary>
        /// Alias to stop listening for the wake word.
        /// </summary>
        public void StopListening()
        {
            _listening = false;
        }

        /// <summary>
        /// Begins a blocking loop reading console input for the wake word.
        /// Exits when StopListening is called.
        /// </summary>
        private void Listen()
        {
            _logger.LogInformation("Listening for wake word...");
            while (_listening)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.ToLower();
                if (input == "jarvis")
                {
                    _logger.LogInformation("Wake word detected.");

                    // 1) Existing command routing
                    _router.HandleCommand();

                    // 2) Notify any subscribers that wake word was detected
                    WakeWordDetected?.Invoke();
                }
            }
            _logger.LogInformation("Stopped listening for wake word.");
        }
    }
}