using Erebus.Core;
using Erebus.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace Erebus.Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of the settings repository
/// </summary>
public sealed class SettingsRepository : ISettingsRepository
{
    private readonly string _connectionString;
    private AppSettings? _cachedSettings;

    public SettingsRepository(IFileSystem fileSystem)
    {
        var dbPath = fileSystem.GetSettingsDatabasePath();
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableSql = """
            CREATE TABLE IF NOT EXISTS settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            )
            """;

        using var command = new SqliteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public async Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var settings = new AppSettings();

        var sql = "SELECT key, value FROM settings";
        using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var key = reader.GetString(0);
            var value = reader.GetString(1);

            switch (key)
            {
                case "Theme":
                    settings.Theme = Enum.Parse<AppTheme>(value);
                    break;
                case "SessionTimeoutMinutes":
                    settings.SessionTimeoutMinutes = int.Parse(value);
                    break;
                case "AutoBackupEnabled":
                    settings.AutoBackupEnabled = bool.Parse(value);
                    break;
                case "BackupIntervalDays":
                    settings.BackupIntervalDays = int.Parse(value);
                    break;
                case "ShowFavicons":
                    settings.ShowFavicons = bool.Parse(value);
                    break;
                case "EnableAnimations":
                    settings.EnableAnimations = bool.Parse(value);
                    break;
                case "LastOpenedVaultId":
                    settings.LastOpenedVaultId = value;
                    break;
                case "UseBiometric":
                    settings.UseBiometric = bool.Parse(value);
                    break;
                case "PinHash":
                    settings.PinHash = value;
                    break;
            }
        }

        _cachedSettings = settings;
        return settings;
    }

    public async Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            var pairs = new Dictionary<string, string>
            {
                { "Theme", settings.Theme.ToString() },
                { "SessionTimeoutMinutes", settings.SessionTimeoutMinutes.ToString() },
                { "AutoBackupEnabled", settings.AutoBackupEnabled.ToString() },
                { "BackupIntervalDays", settings.BackupIntervalDays.ToString() },
                { "ShowFavicons", settings.ShowFavicons.ToString() },
                { "EnableAnimations", settings.EnableAnimations.ToString() },
                { "LastOpenedVaultId", settings.LastOpenedVaultId ?? string.Empty },
                { "UseBiometric", settings.UseBiometric.ToString() },
                { "PinHash", settings.PinHash ?? string.Empty }
            };

            foreach (var pair in pairs)
            {
                var sql = """
                    INSERT OR REPLACE INTO settings (key, value) 
                    VALUES (@key, @value)
                    """;

                using var command = new SqliteCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@key", pair.Key);
                command.Parameters.AddWithValue("@value", pair.Value);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            transaction.Commit();
            _cachedSettings = settings;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
