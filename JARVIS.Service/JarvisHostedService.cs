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
    /// Hosted service for JARVIS: handles startup sequence, optionally streams the Ring camera,
    /// then listens for wake words and routes voice commands to the CommandRouter.
    /// </summary>
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICameraService _cameraService;
        private readonly WebSocketServer _ws;
        private readonly VoiceInput _voiceInput;
        private readonly WakeWordListener _wakeListener;
        private readonly CommandRouter _router;
        private readonly ConversationEngine _conversationEngine;

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            IConfiguration configuration,
            ICameraService cameraService,
            WebSocketServer ws,
            VoiceInput voiceInput,
            WakeWordListener wakeListener,
            CommandRouter router,
            ConversationEngine conversationEngine)
        {
            _logger = logger;
            _configuration = configuration;
            _cameraService = cameraService;
            _ws = ws;
            _voiceInput = voiceInput;
            _wakeListener = wakeListener;
            _router = router;
            _conversationEngine = conversationEngine;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting J.A.R.V.I.S. Service…");

            // Start WebSocket server
            _ws.Start();

            // Optionally stream Ring camera
            if (_configuration.GetValue<bool>("Ring:Enabled"))
            {
                try
                {
                    var frontDoorId = _configuration["Ring:FrontDoorCameraId"];
                    var vodUrl = await _cameraService.GetLiveStreamUrlAsync(frontDoorId);
                    await VoiceOutput.SpeakAsync("Streaming your front door now.");
                    await _ws.BroadcastAsync(new
                    {
                        type = "cameraStream",
                        cameraId = frontDoorId,
                        url = vodUrl
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error streaming Ring camera.");
                    _logger.LogWarning("Continuing without camera stream.");
                }
            }
            else
            {
                _logger.LogInformation("Ring integration disabled; skipping camera stream.");
            }

            // Initialize conversation engine
            _conversationEngine.Initialize();

            // Subscribe to wake-word detection -> start voice listening
            _wakeListener.WakeWordDetected += () =>
            {
                _logger.LogInformation("Wake word detected; ready for voice input.");
                _voiceInput.StartListening();
            };
            _wakeListener.StartListening();

            // Subscribe to voice transcriptions -> route commands
            _voiceInput.TranscriptionReceived += async transcription =>
            {
                _logger.LogInformation("Recognized speech: {Command}", transcription);
                _router.HandleCommand(transcription);
                await VoiceOutput.SpeakAsync("Command executed.");
            };

            _logger.LogInformation("Startup complete; awaiting wake word.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping J.A.R.V.I.S. Service…");

            _voiceInput.StopListening();
            _wakeListener.StopListening();
            _ws.Stop();

            return Task.CompletedTask;
        }
    }
}
