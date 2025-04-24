using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using JARVIS.Modules;
using JARVIS.Modules.Devices.Interfaces;
using KoenZomers.Ring.Api.Exceptions;
using KoenZomers.Ring.Api;

namespace JARVIS.Service
{
    /// <summary>
    /// Hosted service for JARVIS: handles startup sequence, authenticates Ring (with optional 2FA),
    /// initializes core modules, and begins streaming & command routing.
    /// </summary>
    public class JarvisHostedService : IHostedService
    {
        private readonly ILogger<JarvisHostedService> _logger;
        private readonly Session _ringSession;
        private readonly IConfiguration _configuration;
        private readonly ICameraService _cameraService;
        private readonly WebSocketServer _ws;
        private readonly VoiceInput _voiceInput;
        private readonly WakeWordListener _wakeListener;
        private readonly CommandRouter _router;
        private readonly ConversationEngine _conversationEngine;

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            Session ringSession,
            IConfiguration configuration,
            ICameraService cameraService,
            WebSocketServer ws,
            VoiceInput voiceInput,
            WakeWordListener wakeListener,
            CommandRouter router,
            ConversationEngine conversationEngine)
        {
            _logger = logger;
            _ringSession = ringSession;
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

            // 1) Authenticate Ring session with optional 2FA
            try
            {
                _ringSession.Authenticate();
                _logger.LogInformation("Ring session authenticated without 2FA.");
            }
            catch (TwoFactorAuthenticationRequiredException)
            {
                _logger.LogInformation("Two-factor required; reading code from config…");
                var code = _configuration["Ring:TwoFactorCode"];
                if (string.IsNullOrWhiteSpace(code))
                {
                    _logger.LogError("Two-factor code not provided in configuration.");
                    throw new InvalidOperationException(
                        "Two-factor code not provided. Please set Ring:TwoFactorCode in appsettings.json or environment variables.");
                }
                try
                {
                    _ringSession.Authenticate(code);
                    _logger.LogInformation("Ring session authenticated with 2FA.");
                }
                catch (AuthenticationFailedException ex)
                {
                    _logger.LogError(ex, "Ring 2FA authentication failed.");
                    throw new InvalidOperationException("Ring authentication failed after providing 2FA code.", ex);
                }
            }
            catch (AuthenticationFailedException ex)
            {
                _logger.LogError(ex, "Ring authentication failed.");
                throw new InvalidOperationException("Ring authentication failed. Please check Ring:Email and Ring:Password.", ex);
            }

            // 2) Start WebSocket server and voice input
            _ws.Start();
            _voiceInput.StartListening();
            // optional: _wakeListener.StartListening();
            // optional: _conversationEngine.Initialize();

            // 3) Get and broadcast live stream
            var frontDoorId = _configuration["Ring:FrontDoorCameraId"] ?? "123456789";
            string vodUrl;
            try
            {
                vodUrl = await _cameraService.GetLiveStreamUrlAsync(frontDoorId);
            }
            catch (SessionNotAuthenticatedException ex)
            {
                _logger.LogError(ex, "Ring session unexpectedly became unauthenticated during stream request.");
                throw;
            }

            await VoiceOutput.SpeakAsync("Streaming your front door now.");

            await _ws.BroadcastAsync(new
            {
                type = "cameraStream",
                cameraId = frontDoorId,
                url = vodUrl
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping J.A.R.V.I.S. Service…");
            _voiceInput.StopListening();
            _ws.Stop();
            return Task.CompletedTask;
        }
    }
}
