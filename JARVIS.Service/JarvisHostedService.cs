using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using JARVIS.Modules.Devices.Interfaces;
using JARVIS.Modules;

namespace JARVIS.Service
{
    /// <summary>
    /// Hosted service for JARVIS: handles startup sequence, optional Ring camera streaming,
    /// and kicking off voice input and WebSocket server.
    /// </summary>
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;
        private readonly ICameraService _cameraService;
        private readonly WebSocketServer _ws;
        private readonly VoiceInput _voiceInput;
        private readonly WakeWordListener _wakeListener;
        private readonly CommandRouter _router;
        private readonly ConversationEngine _conversationEngine;
        private readonly IConfiguration _config;

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            ICameraService cameraService,
            WebSocketServer ws,
            VoiceInput voiceInput,
            WakeWordListener wakeListener,
            CommandRouter router,
            ConversationEngine conversationEngine,
            IConfiguration config)
        {
            _logger = logger;
            _cameraService = cameraService;
            _ws = ws;
            _voiceInput = voiceInput;
            _wakeListener = wakeListener;
            _router = router;
            _conversationEngine = conversationEngine;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting J.A.R.V.I.S. Service…");

            // Start WebSocket server
            _ws.Start();

            // Conditional Ring camera streaming
            if (_config.GetValue<bool>("Ring:Enabled"))
            {
                _logger.LogInformation("Ring integration is enabled, attempting to stream...");
                try
                {
                    // Use configured camera ID or default
                    var cameraId = _config["Ring:FrontDoorCameraId"] ?? "123456789";
                    var vodUrl = await _cameraService.GetLiveStreamUrlAsync(cameraId);

                    // Begin voice and broadcasting
                    _voiceInput.StartListening();
                    await VoiceOutput.SpeakAsync("Streaming your front door now.");

                    await _ws.BroadcastAsync(new
                    {
                        type = "cameraStream",
                        cameraId,
                        url = vodUrl
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during Ring camera streaming");
                    _logger.LogWarning("Continuing startup without Ring streaming.");

                    _voiceInput.StartListening();
                }
            }
            else
            {
                _logger.LogInformation("Ring integration is disabled, skipping camera startup.");
                _voiceInput.StartListening();
            }

            // Optionally start wake-word and conversation engine
            // _wakeListener.StartListening();
            // _conversationEngine.Initialize();
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
