using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;

namespace JARVIS.Service
{
    /// <summary>
    /// Hosted service that starts up the J.A.R.V.I.S. modules when the application runs.
    /// </summary>
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;

        public JarvisHostedService(ILogger<JarvisHostedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JARVIS Hosted Service is starting.");
            // Initialize core JARVIS modules
            JARVIS.Modules.JARVIS.Startup();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("JARVIS Hosted Service is stopping.");
            // TODO: add any cleanup logic here (e.g., WebSocketServer.Stop())
            return Task.CompletedTask;
        }
    }
}