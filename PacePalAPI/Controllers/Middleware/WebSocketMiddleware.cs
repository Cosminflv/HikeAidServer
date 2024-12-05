using System.Net.WebSockets;
using System.Text;

namespace PacePalAPI.Controllers.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MyWebSocketManager _webSocketManager;

        public WebSocketMiddleware(RequestDelegate next, MyWebSocketManager webSocketManager)
        {
            _next = next;
            _webSocketManager = webSocketManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine($"Request received: {context.Request.Path}");

            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
            {
                var userId = context.Request.Query["userId"];
                if (string.IsNullOrEmpty(userId))
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _webSocketManager.AddConnection(userId!, webSocket);

                await ReceiveMessagesAsync(userId!, webSocket);

                _webSocketManager.RemoveConnection(userId!);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ReceiveMessagesAsync(string userId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var segment = new ArraySegment<byte>(buffer);

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(segment, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Received message from {userId}: {message}");
                }
            }
        }
    }
}
