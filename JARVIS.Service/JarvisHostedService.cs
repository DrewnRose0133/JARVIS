using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;

namespace JARVIS.Service
{
    /// <summary>
    /// Hosted service that initializes and tears down J.A.R.V.I.S. core modules.
    /// </summary>
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;
        private readonly JARVISService _jarvisService;
        private readonly VoiceInput _voiceInput;
        private readonly WakeWordListener _wakeWordListener;

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            JARVISService jarvisService,
            VoiceInput voiceInput,
            WakeWordListener wakeWordListener)
        {
            _logger = logger;
            _jarvisService = jarvisService;
            _voiceInput = voiceInput;
            _wakeWordListener = wakeWordListener;
        }

        /// <summary>
        /// Called when the host is ready to start the service.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("J.A.R.V.I.S. Hosted Service is starting.");

            // Start all core modules
            _jarvisService.Start();

            // then begin listening for voice commands
            _voiceInput.StartListening();

            _wakeWordListener.Listen();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the host is performing a graceful shutdown.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("J.A.R.V.I.S. Hosted Service is stopping.");

            // Stop all core modules
            _jarvisService.Stop();
            _voiceInput.StopListening();

            
            return Task.CompletedTask;
        }
    }
}
