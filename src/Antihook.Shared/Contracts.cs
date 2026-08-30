using System.Text.Json.Serialization;

namespace Antihook.Shared;

public record ServerInfo(string Name, string Map, int Players, int MaxPlayers, int Ping);
public record LoginRequest(string Action, string Username, string Password, string Hwid);
public record LoginResponse(bool Success, string Message, string? Token = null);
public record Envelope(string Type, object Payload);

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponse))]
[JsonSerializable(typeof(List<ServerInfo>))]
public partial class JsonContext : JsonSerializerContext { }
