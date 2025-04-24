using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    /// <summary>
    /// Orchestrator for core J.A.R.V.I.S. services and modules.
    /// </summary>
    public class JARVISService
    {
        private readonly WebSocketServer _webSocketServer;
        private readonly ConversationEngine _conversationEngine;
        private readonly ILogger<JARVISService> _logger;

        public JARVISService(
            WebSocketServer webSocketServer,
            ConversationEngine conversationEngine,
            ILogger<JARVISService> logger)
        {
            _webSocketServer = webSocketServer;
            _conversationEngine = conversationEngine;
            _logger = logger;
        }

        /// <summary>
        /// Starts all core modules.
        /// </summary>
        public void Start()
        {
            _logger.LogInformation("Starting J.A.R.V.I.S. modules...");
            _webSocketServer.Start();
            // Initialize conversation engine or other modules as needed
            // e.g., _conversationEngine.SetReminder("Check systems", 5);
        }

        /// <summary>
        /// Stops all core modules.
        /// </summary>
        public void Stop()
        {
            _logger.LogInformation("Stopping J.A.R.V.I.S. modules...");
            _webSocketServer.Stop();
            // Cleanup other modules if necessary
        }
    }
}
