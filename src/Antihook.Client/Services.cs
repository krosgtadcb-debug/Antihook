using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Antihook.Shared;

namespace Antihook.Client;

internal sealed class WebSocketGateway : IAsyncDisposable
{
    private readonly ClientWebSocket socket = new();
    public async Task<LoginResponse> LoginAsync(string user, string password, string hwid, CancellationToken cancellationToken = default)
    {
        await socket.ConnectAsync(new Uri("ws://localhost:5050/"), cancellationToken);
        var request = JsonSerializer.Serialize(new { type = "login", payload = new LoginRequest("login", user, password, hwid) });
        await socket.SendAsync(Encoding.UTF8.GetBytes(request), WebSocketMessageType.Text, true, cancellationToken);
        var buffer = new byte[4096]; var result = await socket.ReceiveAsync(buffer, cancellationToken);
        return JsonSerializer.Deserialize<LoginResponse>(Encoding.UTF8.GetString(buffer, 0, result.Count)) ?? new(false, "Respuesta inválida");
    }
    public async ValueTask DisposeAsync() { socket.Dispose(); await Task.CompletedTask; }
}

internal sealed class AdminDashboard : Form
{
    private readonly DataGridView users = new() { Dock = DockStyle.Fill, BackgroundColor = Color.FromArgb(23, 26, 33), ForeColor = Color.White, AllowUserToAddRows = false, RowHeadersVisible = false };
    private readonly RichTextBox logs = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 18, 24), ForeColor = Color.LightGreen, ReadOnly = true };
    public AdminDashboard()
    {
        Text = "Antihook // Administración"; BackColor = Color.FromArgb(27, 40, 56); ForeColor = Color.White; Width = 900; Height = 520;
        users.Columns.Add("name", "NOMBRE"); users.Columns.Add("hwid", "HWID"); users.Columns.Add("ip", "IP"); users.Columns.Add("time", "TIEMPO"); users.Columns.Add("actions", "ACCIONES");
        users.Rows.Add("DemoUser", "A1B2C3D4…", "127.0.0.1", "00:12:34", "KICK / BAN");
        users.CellContentClick += (_, e) => { if (e.RowIndex >= 0) AddLog($"Acción administrativa solicitada para {users.Rows[e.RowIndex].Cells[0].Value}"); };
        var tabs = new TabControl { Dock = DockStyle.Fill }; var usersTab = new TabPage("Usuarios conectados") { BackColor = users.BackColor }; usersTab.Controls.Add(users); var logsTab = new TabPage("Logs") { BackColor = logs.BackColor }; logsTab.Controls.Add(logs); tabs.TabPages.Add(usersTab); tabs.TabPages.Add(logsTab); Controls.Add(tabs); AddLog("Panel administrativo iniciado");
    }
    private void AddLog(string message) => logs.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
}
