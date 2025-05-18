using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PacePalAPI.Models;
using PacePalAPI.Requests;
using PacePalAPI.Services;
using PacePalAPI.Services.UserService;

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

                // Retrieve scoped services from the context
                var hikeService = context.RequestServices.GetRequiredService<IUserCollectionService>();

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _webSocketManager.AddConnection(userId!, webSocket);

                await ReceiveMessagesAsync(userId!, webSocket, hikeService);

                _webSocketManager.RemoveConnection(userId!);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ReceiveMessagesAsync(string userId, WebSocket webSocket, IUserCollectionService userService)
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

                    try
                    {
                        // Deserialize the coordinate
                        var coordinate = JsonSerializer.Deserialize<CoordinatePredictionDto>(message);
                        if (!int.TryParse(userId, out int userIdInt))
                        {
                            Console.WriteLine($"Invalid user ID: {userId}");
                            continue;
                        }

                        // Retrieve the active hike
                        var activeHike = await userService.GetActiveHike(userIdInt);
                        if (activeHike == null)
                        {
                            Console.WriteLine($"No active hike found for user {userId}");
                            continue;
                        }

                        Coordinate coordToAdd = new Coordinate
                        {
                            Latitude = coordinate!.Latitude,
                            Longitude = coordinate.Longitude,
                            Elevation = coordinate.Elevation,
                            Timestamp = coordinate.Time,
                            TrackCoordinatesConfirmedCurrentHikeId = activeHike.Id
                        };

                        // Add coordinate and save
                        activeHike.UserProgressCoordinates.Add(coordToAdd);
                        await userService.UpdateHikeProgress(activeHike);
                        Console.WriteLine($"Added coordinate to user {userId}'s active hike.");
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                }
            }
        }
    }
}