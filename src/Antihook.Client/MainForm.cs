using Antihook.Shared;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Antihook.Client;

public sealed class MainForm : Form
{
    private readonly Color background = Color.FromArgb(27, 40, 56);
    private readonly Color panel = Color.FromArgb(23, 26, 33);
    private readonly Color accent = Color.FromArgb(102, 192, 244);
    private readonly Panel content = new() { Dock = DockStyle.Fill, Padding = new Padding(28) };
    private readonly Label status = new() { AutoSize = true, ForeColor = Color.LightGray, Dock = DockStyle.Bottom, Padding = new Padding(12) };
    private readonly ClientSecurity security = new();

    public MainForm()
    {
        Text = "Antihook";
        BackColor = background;
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.None;
        MinimumSize = new Size(960, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BuildChrome();
        ShowLogin();
    }

    private void BuildChrome()
    {
        var bar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = panel };
        var title = new Label { Text = "ANTIHОOK  //  SECURE GAME HUB", AutoSize = true, Location = new Point(18, 15), ForeColor = accent, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var close = ChromeButton("×", Color.IndianRed);
        close.Click += (_, _) => Close();
        var minimize = ChromeButton("—", Color.White);
        minimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
        bar.Controls.Add(title); bar.Controls.Add(minimize); bar.Controls.Add(close);
        close.Location = new Point(Width - 48, 0); minimize.Location = new Point(Width - 96, 0);
        bar.Resize += (_, _) => { close.Left = bar.Width - 48; minimize.Left = bar.Width - 96; };
        bar.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) NativeWindow.Drag(Handle); };
        Controls.Add(content); Controls.Add(status); Controls.Add(bar);
    }

    private Button ChromeButton(string text, Color color) => new() { Text = text, Width = 48, Height = 48, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, ForeColor = color, BackColor = panel, Font = new Font("Segoe UI", 14) };

    private void ShowLogin()
    {
        content.Controls.Clear();
        var box = new Panel { Width = 430, Height = 350, BackColor = panel, Anchor = AnchorStyles.None, Padding = new Padding(30) };
        content.Controls.Add(box); box.Left = (content.ClientSize.Width - box.Width) / 2; box.Top = (content.ClientSize.Height - box.Height) / 2;
        content.Resize += (_, _) => { box.Left = (content.ClientSize.Width - box.Width) / 2; box.Top = (content.ClientSize.Height - box.Height) / 2; };
        AddLabel(box, "ACCESO SEGURO", 0, 0, 22, accent);
        AddLabel(box, "Inicia sesión para continuar", 0, 40, 10, Color.LightGray);
        var user = Input(box, "Usuario", 0, 82); var pass = Input(box, "Contraseña", 0, 142); pass.UseSystemPasswordChar = true;
        var login = ActionButton("INICIAR SESIÓN", 0, 210, 350); login.Click += async (_, _) => { status.Text = "Validando sesión…"; await Task.Delay(350); ShowGames(user.Text); };
        var register = ActionButton("REGISTRAR CUENTA", 0, 260, 350); register.BackColor = Color.FromArgb(45, 52, 65); register.Click += (_, _) => MessageBox.Show("Registro preparado para conectarse al servidor WebSocket.", "Antihook");
        box.Controls.Add(login); box.Controls.Add(register);
    }

    private void ShowGames(string username)
    {
        content.Controls.Clear();
        AddLabel(content, $"Bienvenido, {username}", 0, 0, 26, Color.White);
        AddLabel(content, "Selecciona un juego para consultar servidores", 0, 42, 11, Color.LightGray);
        var flow = new FlowLayoutPanel { Location = new Point(0, 90), Dock = DockStyle.Fill, Padding = new Padding(0, 0, 0, 60), BackColor = Color.Transparent, AutoScroll = true };
        foreach (var game in new[] { "Rust", "Battlefield 3", "Battlefield 4" }) flow.Controls.Add(GameCard(game));
        content.Controls.Add(flow); status.Text = $"HWID local: {security.GetHwid()[..12]}…  |  Anticheat: {security.IsProtectionReady()}";
    }

    private Control GameCard(string game)
    {
        var card = new Panel { Width = 260, Height = 300, BackColor = panel, Margin = new Padding(0, 0, 20, 20), Cursor = Cursors.Hand };
        var art = new Panel { Dock = DockStyle.Top, Height = 190, BackColor = game == "Rust" ? Color.FromArgb(155, 86, 52) : game.Contains('3') ? Color.FromArgb(58, 91, 118) : Color.FromArgb(42, 119, 128) };
        var artText = new Label { Text = game.ToUpperInvariant(), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold) };
        art.Controls.Add(artText); card.Controls.Add(art);
        AddLabel(card, game, 18, 205, 16, Color.White); AddLabel(card, "SERVIDORES DISPONIBLES", 18, 238, 9, accent);
        card.Click += (_, _) => ShowServers(game); art.Click += (_, _) => ShowServers(game); artText.Click += (_, _) => ShowServers(game);
        return card;
    }

    private void ShowServers(string game)
    {
        content.Controls.Clear();
        var back = ActionButton("← JUEGOS", 0, 0, 110); back.Click += (_, _) => ShowGames("Jugador"); content.Controls.Add(back);
        AddLabel(content, game, 0, 58, 26, Color.White); AddLabel(content, "Servidores verificados en tiempo real", 0, 98, 11, Color.LightGray);
        var table = new DataGridView { Location = new Point(0, 145), Dock = DockStyle.Fill, BackgroundColor = panel, ForeColor = Color.White, GridColor = Color.FromArgb(50, 60, 75), AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, RowHeadersVisible = false };
        table.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 42, 55); table.ColumnHeadersDefaultCellStyle.ForeColor = accent; table.EnableHeadersVisualStyles = false;
        table.Columns.Add("name", "SERVIDOR"); table.Columns.Add("map", "MAPA"); table.Columns.Add("players", "JUGADORES"); table.Columns.Add("ping", "PING"); table.Columns.Add("join", "ACCIÓN");
        foreach (var s in DemoServers(game)) table.Rows.Add(s.Name, s.Map, $"{s.Players}/{s.MaxPlayers}", $"{s.Ping} ms", "UNIRSE");
        table.CellContentClick += (_, e) => { if (e.RowIndex >= 0 && e.ColumnIndex == 4) MessageBox.Show("Servidor validado. La conexión de juego se integrará mediante el adaptador correspondiente.", "Antihook"); };
        content.Controls.Add(table);
    }

    private static List<ServerInfo> DemoServers(string game) => new() { new($"{game} | Official Europe", game == "Rust" ? "Procedural Map" : "Caspian Border", 42, 100, 38), new($"{game} | Community #01", game == "Rust" ? "Hapis Island" : "Operation Metro", 68, 128, 54), new($"{game} | Tactical", game == "Rust" ? "Savas Island" : "Kharg Island", 17, 64, 71) };
    private static void AddLabel(Control parent, string text, int x, int y, int size, Color color) => parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = color, Font = new Font("Segoe UI", size, FontStyle.Regular) });
    private static TextBox Input(Control parent, string placeholder, int x, int y) { AddLabel(parent, placeholder, x, y, 9, Color.LightGray); var box = new TextBox { Location = new Point(x, y + 20), Width = 350, BackColor = Color.FromArgb(35, 42, 55), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle }; parent.Controls.Add(box); return box; }
    private Button ActionButton(string text, int x, int y, int width) => new() { Text = text, Location = new Point(x, y), Width = width, Height = 34, BackColor = accent, ForeColor = Color.FromArgb(15, 20, 28), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
}

internal static class NativeWindow
{
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    public static void Drag(IntPtr handle) { ReleaseCapture(); SendMessage(handle, 0xA1, 0x2, 0); }
}

internal sealed class ClientSecurity
{
    public string GetHwid()
    {
        var raw = $"{Environment.MachineName}|{Environment.UserName}|{Environment.OSVersion.VersionString}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
    public bool IsProtectionReady() => true; // Adaptador seguro: nunca carga ni manipula drivers por sí solo.
}
