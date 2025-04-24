using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JARVIS.Modules;
using JARVIS.Modules.Devices.Interfaces;

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

        private readonly ICameraService _cameraService;
      

        public JarvisHostedService(
            ILogger<JarvisHostedService> logger,
            ICameraService cameraService,
            WebSocketServer ws,
            VoiceInput voiceInput,
            WakeWordListener wakeListener,
            CommandRouter router,
            ConversationEngine conversationEngine)
        {
            _logger = logger;
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

            _ws.Start();

            // Get a list of devices, then find the ID you want to pass in below
            // var id = _ringSession.GetRingDevices();

            var frontDoorId = "123456789";
            var vodUrl = await _cameraService.GetLiveStreamUrlAsync(frontDoorId);
            // _wakeListener.StartListening();
            _voiceInput.StartListening();
            // The wake listener will route commands into CommandRouter
            // _conversationEngine.Initialize();

            await VoiceOutput.SpeakAsync("Streaming your front door now.");

            // broadcast to all Visualizers
            await _ws.BroadcastAsync(new
            {
                type = "cameraStream",
                cameraId = frontDoorId,
                url = vodUrl
            });

            //return Task;
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
