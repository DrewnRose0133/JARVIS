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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting J.A.R.V.I.S. Service…");

            // 1) WebSockets
            _ws.Start();

            // 2) Ring camera (fire-and-forget)
            if (_configuration.GetValue<bool>("Ring:Enabled"))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var frontDoorId = _configuration["Ring:FrontDoorCameraId"];
                        var vodUrl = await _cameraService.GetLiveStreamUrlAsync(frontDoorId);
                        await VoiceOutput.SpeakAsync("Streaming your front door now.");
                        await _ws.BroadcastAsync(new { type = "cameraStream", cameraId = frontDoorId, url = vodUrl });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ring streaming failed, continuing without it.");
                    }
                });
            }

            // 3) Conversation engine setup
            _conversationEngine.Initialize();

            // 4) Wake-word listener on its own thread
            _wakeListener.WakeWordDetected += () =>
            {
                _logger.LogInformation("Wake word detected; starting voice input…");
                _voiceInput.StartListening();
            };
            _ = Task.Run(() => _wakeListener.StartListening());

            // 5) Route every transcription into your engine
            _voiceInput.TranscriptionReceived += async transcription =>
            {
                _logger.LogInformation("Heard: {0}", transcription);
                await _conversationEngine.ProcessTranscriptionAsync(transcription);
            };
            // You can start voice listening immediately if you want:
            // _voiceInput.StartListening();

            _logger.LogInformation("J.A.R.V.I.S. is ready—awaiting your voice.");
            return Task.CompletedTask;
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
