using Microsoft.Data.Sqlite;

namespace Antihook.Server;

public sealed class Database
{
    private readonly string connectionString;
    public Database(string path = "server.db") => connectionString = $"Data Source={path}";

    public void Initialize()
    {
        using var connection = new SqliteConnection(connectionString); connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY, username TEXT UNIQUE NOT NULL, password_hash TEXT NOT NULL, hwid TEXT NOT NULL, last_ip TEXT, registered_at TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS bans (id INTEGER PRIMARY KEY, hwid TEXT UNIQUE NOT NULL, reason TEXT, created_at TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS logs (id INTEGER PRIMARY KEY, event TEXT NOT NULL, created_at TEXT NOT NULL);
            """;
        command.ExecuteNonQuery();
    }

    public void Log(string message)
    {
        using var connection = new SqliteConnection(connectionString); connection.Open(); using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO logs(event, created_at) VALUES ($event, $date)"; cmd.Parameters.AddWithValue("$event", message); cmd.Parameters.AddWithValue("$date", DateTimeOffset.UtcNow.ToString("O")); cmd.ExecuteNonQuery();
    }

    public bool IsBanned(string hwid)
    {
        using var connection = new SqliteConnection(connectionString); connection.Open(); using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM bans WHERE hwid = $hwid"; cmd.Parameters.AddWithValue("$hwid", hwid); return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
