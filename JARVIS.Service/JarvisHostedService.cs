using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;

namespace JARVIS.Service
{
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;
        private readonly WebSocketServer _ws;
        private readonly VoiceInput _voiceInput;
        private readonly WakeWordListener _wakeListener;
        private readonly CommandRouter _router;
        private readonly ConversationEngine _conversationEngine;

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            WebSocketServer ws,
            VoiceInput voiceInput,
            WakeWordListener wakeListener,
            CommandRouter router,
            ConversationEngine conversationEngine)
        {
            _logger = logger;
            _ws = ws;
            _voiceInput = voiceInput;
            _wakeListener = wakeListener;
            _router = router;
            _conversationEngine = conversationEngine;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting J.A.R.V.I.S. Service…");

            _ws.Start();
           // _wakeListener.StartListening();
            _voiceInput.StartListening();
            // The wake listener will route commands into CommandRouter
           // _conversationEngine.Initialize();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping J.A.R.V.I.S. Service…");

            _voiceInput.StopListening();
           // _wakeListener.StopListening();
            _ws.Stop();

            return Task.CompletedTask;
        }
    }
}
