using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    public class WebSocketServer
    {
        private readonly HttpListener _listener;
        private readonly ILogger<WebSocketServer> _logger;
        private CancellationTokenSource _cts;

        public WebSocketServer(ILogger<WebSocketServer> logger)
        {
            _logger = logger;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:5005/ws/");
        }

        public void Start()
        {
            _listener.Start();
            _logger.LogInformation("WebSocket server listening on ws://localhost:5005/ws/");
            _cts = new CancellationTokenSource();
            _ = RunAsync(_cts.Token);
        }

        private async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException)
                {
                    break; // listener was stopped
                }

                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleConnection(wsContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleConnection(WebSocket socket)
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation("WebSocket ► {Message}", msg);
                    // TODO: broadcast to other clients if desired
                }
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener.Stop();
            _logger.LogInformation("WebSocket server stopped.");
        }
    }
}
