using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Antihook.Shared;

namespace Antihook.Server;

public static class Program
{
    public static async Task Main()
    {
        var database = new Database(); database.Initialize();
        using var listener = new HttpListener(); listener.Prefixes.Add("http://localhost:5050/"); listener.Start();
        Console.WriteLine("Antihook server listening on ws://localhost:5050/"); database.Log("Servidor iniciado");
        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(() => Handle(context, database));
        }
    }

    private static async Task Handle(HttpListenerContext context, Database database)
    {
        if (!context.Request.IsWebSocketRequest) { context.Response.StatusCode = 400; context.Response.Close(); return; }
        var socket = (await context.AcceptWebSocketAsync(null)).WebSocket; database.Log($"Conexión entrante: {context.Request.RemoteEndPoint}");
        var buffer = new byte[8192];
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                using var doc = JsonDocument.Parse(json); var type = doc.RootElement.GetProperty("type").GetString();
                if (type == "login")
                {
                    var request = JsonSerializer.Deserialize<LoginRequest>(doc.RootElement.GetProperty("payload").GetRawText());
                    var valid = request is not null && database.ValidateUser(request.Username, request.Password, request.Hwid);
                    var response = valid ? new LoginResponse(true, "Autenticación correcta", Guid.NewGuid().ToString("N")) : new LoginResponse(false, "Datos incorrectos");
                    database.Log($"Autenticación {(response.Success ? "exitosa" : "rechazada")}"); await Send(socket, response);
                }
                else if (type == "servers") await Send(socket, new List<ServerInfo> { new("Official Europe", "Procedural Map", 42, 100, 38), new("Community #01", "Hapis Island", 68, 128, 54) });
            }
        }
        catch (Exception ex) { database.Log($"Error de conexión: {ex.Message}"); }
        finally { if (socket.State == WebSocketState.Open) await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None); }
    }

    private static Task Send<T>(WebSocket socket, T value) { var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)); return socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None); }
}
