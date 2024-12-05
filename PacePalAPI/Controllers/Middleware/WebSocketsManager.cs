using System.Net.WebSockets;
using System.Text;

namespace PacePalAPI.Controllers.Middleware
{
    public class MyWebSocketManager
    {
        private readonly Dictionary<string, WebSocket> _connections = new();

        public void AddConnection(string userId, WebSocket webSocket)
        {
            lock (_connections)
            {
                _connections[userId] = webSocket;
            }
        }

        public void RemoveConnection(string userId)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(userId))
                {
                    _connections[userId].CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                    _connections.Remove(userId);
                }
            }
        }

        public async Task SendMessageAsync(string userId, string message)
        {
            if (_connections.TryGetValue(userId, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                var buffer = new ArraySegment<byte>(bytes);
                await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
