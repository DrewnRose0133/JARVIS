using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JARVIS.Modules
{
    public class WebSocketServer
    {
        private readonly HttpListener _listener;
        private readonly ILogger<WebSocketServer> _logger;
        private readonly List<WebSocket> _clients = new();
        private readonly object _clientsLock = new();
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
                    var socket = wsContext.WebSocket;

                    // track this client
                    lock (_clientsLock)
                        _clients.Add(socket);

                    // handle it
                    _ = HandleConnection(socket);
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
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing",
                            CancellationToken.None);
                    }
                    else
                    {
                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation("WebSocket ► {Message}", msg);
                        // TODO: potentially broadcast or process this incoming message
                    }
                }
            }
            catch (WebSocketException) { /* connection lost */ }
            finally
            {
                // remove from our list
                lock (_clientsLock)
                    _clients.Remove(socket);
            }
        }

        /// <summary>
        /// Broadcasts an object to all connected clients as JSON.
        /// </summary>
        public async Task BroadcastAsync<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            var buffer = new ArraySegment<byte>(bytes);

            List<WebSocket> clientsCopy;
            lock (_clientsLock)
            {
                clientsCopy = _clients
                    .Where(ws => ws.State == WebSocketState.Open)
                    .ToList();
            }

            var tasks = clientsCopy
                .Select(ws => ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None));

            await Task.WhenAll(tasks);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener.Stop();
            _logger.LogInformation("WebSocket server stopped.");
        }
    }
}
