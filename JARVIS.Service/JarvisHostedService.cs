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
    /// Hosted service for JARVIS: handles startup sequence, optional Ring streaming,
    /// and debounced voice command processing triggered by wake word or continuous listening.
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

        // CTS for debouncing rapid speech events
        private CancellationTokenSource _speechDebounceCts;

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

            // 1) Start WebSocket server
            _ws.Start();

            // 2) Optional Ring camera streaming (fire-and-forget)
            if (_configuration.GetValue<bool>("Ring:Enabled"))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var cameraId = _configuration["Ring:FrontDoorCameraId"];
                        var url = await _cameraService.GetLiveStreamUrlAsync(cameraId);
                        await VoiceOutput.SpeakAsync("Streaming your front door now.");
                        await _ws.BroadcastAsync(new { type = "cameraStream", cameraId, url });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ring streaming failed, continuing without it.");
                    }
                });
            }

            // 3) Initialize conversation context
            _conversationEngine.Initialize();

            // 4) Debounced transcription handling
            _voiceInput.TranscriptionReceived += text =>
            {
                // Cancel pending debounce
                _speechDebounceCts?.Cancel();
                _speechDebounceCts = new CancellationTokenSource();
                var cts = _speechDebounceCts;

                // Debounce: wait 1.5s after final utterance
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1.5), cts.Token);
                        _logger.LogInformation("Processing debounced speech: {Text}", text);

                        var reply = await _conversationEngine.ProcessTranscriptionAsync(text);
                        await VoiceOutput.SpeakAsync(reply);
                    }
                    catch (TaskCanceledException)
                    {
                        // A new transcription arrived; skip this attempt
                    }
                });
            };

            // 5) Wake-word listener triggers voice listening
            _wakeListener.WakeWordDetected += () =>
            {
                _logger.LogInformation("Wake word detected; starting voice input.");
                _voiceInput.StartListening();
            };
            _ = Task.Run(() => _wakeListener.StartListening());

            // Optionally start continuous listening as well:
            // _voiceInput.StartListening();

            _logger.LogInformation("J.A.R.V.I.S. is ready—awaiting wake word or speech.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping J.A.R.V.I.S. Service…");
            _speechDebounceCts?.Cancel();
            _voiceInput.StopListening();
            _wakeListener.StopListening();
            _ws.Stop();
            return Task.CompletedTask;
        }
    }
}
