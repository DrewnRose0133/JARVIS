
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JARVIS.Modules
{
    public static class WebSocketServer
    {
        public static async void Start()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5005/ws/");
            listener.Start();
            Logger.Log("WebSocket server started on port 5005.");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = EchoLoop(wsContext.WebSocket);
                }
            }
        }

        private static async Task EchoLoop(WebSocket socket)
        {
            byte[] buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                else
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Logger.Log("WebSocket Received: " + msg);
                }
            }
        }

        public static async Task SendMessage(string message)
        {
            // To implement: manage connected clients and broadcast messages to the visualizer
        }
    }
}
