using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    // Map channel -> (userId -> list of SSE connections)
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, List<StreamWriter>>> ChannelClients
        = new();

    /// <summary>
    /// SSE endpoint per channel (alerts or friendships) and user
    /// </summary>
    [HttpGet]
    [Route("stream/{channel}")]
    public async Task StreamEvents([FromRoute] string channel, [FromQuery] string userId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(channel))
        {
            Response.StatusCode = 400;
            await Response.WriteAsync("Missing channel or userId");
            return;
        }

        var cancellationToken = HttpContext.RequestAborted;

        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        var streamWriter = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);

        // Ensure channel dictionary exists
        var userDict = ChannelClients.GetOrAdd(channel, _ => new ConcurrentDictionary<string, List<StreamWriter>>());
        // Get or create list for this user
        var connections = userDict.GetOrAdd(userId, _ => new List<StreamWriter>());

        lock (connections)
        {
            connections.Add(streamWriter);
        }

        try
        {
            // Keep connection alive until client disconnects
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Client disconnected
        }
        finally
        {
            // Cleanup
            lock (connections)
            {
                connections.Remove(streamWriter);
                if (connections.Count == 0)
                {
                    userDict.TryRemove(userId, out _);
                }
            }
            await streamWriter.DisposeAsync();
        }
    }

    /// <summary>
    /// Send an SSE message for a specific channel and user
    /// </summary>
    public static async Task SendFriendRequestToUser(string channel, string userId, string eventName, object data)
    {
        if (!ChannelClients.TryGetValue(channel, out var userDict))
            return;

        if (!userDict.TryGetValue(userId, out var connections))
            return;

        var payload = BuildSseEvent(eventName, data);
        var tasks = new List<Task>();
        var disconnected = new List<StreamWriter>();

        lock (connections)
        {
            foreach (var client in connections)
            {
                // Schedule async write
                tasks.Add(WriteToClientAsync(client, payload, disconnected));
            }

            // Remove dead connections once all writes have completed
            Task.WhenAll(tasks).ContinueWith(_ =>
            {
                lock (connections)
                {
                    foreach (var dead in disconnected)
                        connections.Remove(dead);
                    if (connections.Count == 0)
                        userDict.TryRemove(userId, out List<StreamWriter> _);
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Broadcast an alert to all connected users in the alerts channel
    /// </summary>
    public static async Task BroadcastAlertAsync(object alertData)
    {
        const string channel = "alerts";
        if (!ChannelClients.TryGetValue(channel, out var userDict))
            return;

        var payload = BuildSseEvent("alert", alertData);
        var tasks = new List<Task>();

        foreach (var kvp in userDict)
        {
            var connections = kvp.Value;
            var disconnected = new List<StreamWriter>();

            lock (connections)
            {
                foreach (var client in connections)
                {
                    // schedule async write
                    tasks.Add(WriteToClientAsync(client, payload, disconnected));
                }

                // remove dead connections once WriteToClientAsync has run
                Task.WhenAll(tasks).ContinueWith(_ =>
                {
                    lock (connections)
                    {
                        foreach (var dead in disconnected)
                            connections.Remove(dead);
                        if (connections.Count == 0)
                            userDict.TryRemove(kvp.Key, out List<StreamWriter> _);
                    }
                });
            }
        }

        await Task.WhenAll(tasks);
    }

    private static async Task WriteToClientAsync(StreamWriter client, string payload, List<StreamWriter> disconnected)
    {
        try
        {
            await client.WriteAsync(payload);
            await client.FlushAsync();
        }
        catch
        {
            lock (disconnected)
            {
                disconnected.Add(client);
            }
        }
    }

    private static string BuildSseEvent(string eventName, object data)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(eventName))
        {
            sb.Append($"event: {eventName}\n");
        }
        var json = JsonSerializer.Serialize(data);
        sb.Append($"data: {json}\n\n"); // Single data line
        return sb.ToString();
    }

    // Convenience methods
    public static async Task SendFriendshipRequest(string userId, object requestData)
        => await SendFriendRequestToUser("friendships", userId, "friendRequest", requestData);

    public static async Task SendAlertToAll(object alertData)
        => await BroadcastAlertAsync(alertData);
}