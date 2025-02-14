using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private static readonly List<StreamWriter> Clients = new();

    [HttpGet]
    [Route("stream")]
    public async Task StreamEvents(CancellationToken cancellationToken)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");

        var streamWriter = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);
        lock (Clients)
        {
            Clients.Add(streamWriter);
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                await Task.Delay(1000, cancellationToken); // Delay 1000ms
            }
        }
        catch (TaskCanceledException)
        {
            // Handle disconnections
        }
        finally
        {
            lock (Clients)
            {
                Clients.Remove(streamWriter);
            }
            await streamWriter.DisposeAsync();
        }
    }

    public static void SendSseMessage(object message)
    {
        string jsonMessage = $"{JsonSerializer.Serialize(message)}\n\n";
        List<StreamWriter> disconnectedClients = new();

        lock (Clients)
        {
            foreach (var client in Clients)
            {
                try
                {
                    client.WriteLineAsync(jsonMessage).Wait();
                    client.FlushAsync().Wait();
                }
                catch (Exception)
                {
                    disconnectedClients.Add(client);
                }
            }

            // Remove disconnected clients
            foreach (var client in disconnectedClients)
            {
                Clients.Remove(client);
            }
        }
    }

    public static async Task SendSseMessageAsync(object message)
    {
        string jsonMessage = $"{JsonSerializer.Serialize(message)}\n\n";

        // Create a snapshot of clients to iterate safely
        List<StreamWriter> clientsSnapshot;
        lock (Clients)
        {
            clientsSnapshot = Clients.ToList(); // Copy while locked
        }

        var disconnectedClients = new List<StreamWriter>();

        // Process the snapshot asynchronously (no lock held here)
        foreach (var client in clientsSnapshot)
        {
            try
            {
                await client.WriteLineAsync(jsonMessage).ConfigureAwait(false);
                await client.FlushAsync().ConfigureAwait(false);
            }
            catch (IOException) // Detect broken pipes
            {
                disconnectedClients.Add(client);
            }
            catch (ObjectDisposedException) // Client already disposed
            {
                disconnectedClients.Add(client);
            }
        }

        // Remove disconnected clients from the original list
        if (disconnectedClients.Count > 0)
        {
            lock (Clients)
            {
                foreach (var client in disconnectedClients)
                {
                    Clients.Remove(client);
                }
            }
        }
    }
}
