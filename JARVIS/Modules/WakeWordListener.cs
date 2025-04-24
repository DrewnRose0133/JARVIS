using System;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Listens for the "jarvis" wake word and routes commands accordingly.
    /// </summary>
    public class WakeWordListener
    {
        private readonly CommandRouter _router;
        private readonly ILogger<WakeWordListener> _logger;

        public WakeWordListener(CommandRouter router, ILogger<WakeWordListener> logger)
        {
            _router = router;
            _logger = logger;
        }

        /// <summary>
        /// Begins a blocking loop reading console input for the wake word.
        /// </summary>
        public void Listen()
        {
            _logger.LogInformation("Listening for wake word...");
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.ToLower();
                if (input == "jarvis")
                {
                    _logger.LogInformation("Wake word detected.");
                    _router.HandleCommand();
                }
            }
        }
    }
}
