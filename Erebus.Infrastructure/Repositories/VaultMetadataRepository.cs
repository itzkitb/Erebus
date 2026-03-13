using Erebus.Core;
using Erebus.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace Erebus.Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of the vault metadata repository
/// </summary>
public sealed class VaultMetadataRepository : IVaultMetadataRepository
{
    private readonly string _connectionString;
    private readonly IFileSystem _fileSystem;

    public VaultMetadataRepository(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        var dbPath = fileSystem.GetVaultsDatabasePath();
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
            CREATE TABLE IF NOT EXISTS vaults (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                path TEXT NOT NULL,
                salt BLOB NOT NULL,
                iv BLOB NOT NULL,
                version TEXT NOT NULL DEFAULT '1.0.0',
                password_hint TEXT,
                icon TEXT,
                created_at TEXT NOT NULL,
                modified_at TEXT NOT NULL,
                biometric_enabled INTEGER NOT NULL DEFAULT 0,
                biometric_key BLOB,
                is_default INTEGER NOT NULL DEFAULT 0
            )
            """;

        using var command = new SqliteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public async Task<IReadOnlyList<VaultInfo>> GetAllVaultsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM vaults ORDER BY modified_at DESC";
        var vaults = new List<VaultInfo>();

        using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            vaults.Add(ReadVaultInfo(reader));
        }

        return vaults;
    }

    public async Task<VaultInfo?> GetVaultByIdAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM vaults WHERE id = @id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", vaultId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return ReadVaultInfo(reader);
        }

        return null;
    }

    public async Task<VaultInfo> CreateVaultAsync(VaultInfo vault, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            INSERT INTO vaults (id, name, path, salt, iv, version, password_hint, icon, created_at, modified_at, biometric_enabled, biometric_key)
            VALUES (@id, @name, @path, @salt, @iv, @version, @passwordHint, @icon, @createdAt, @modifiedAt, @biometricEnabled, @biometricKey)
            """;

        using var command = new SqliteCommand(sql, connection);
        AddVaultParameters(command, vault);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return vault;
    }

    public async Task UpdateVaultAsync(VaultInfo vault, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            UPDATE vaults 
            SET name = @name, path = @path, salt = @salt, iv = @iv, version = @version,
                password_hint = @passwordHint, icon = @icon, modified_at = @modifiedAt,
                biometric_enabled = @biometricEnabled, biometric_key = @biometricKey
            WHERE id = @id
            """;

        using var command = new SqliteCommand(sql, connection);
        AddVaultParameters(command, vault);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "DELETE FROM vaults WHERE id = @id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", vaultId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<VaultInfo?> GetDefaultVaultAsync(CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM vaults WHERE is_default = 1 LIMIT 1";
        using var command = new SqliteCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return ReadVaultInfo(reader);
        }

        return null;
    }

    public async Task SetDefaultVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            // Clear defaults
            var clearSql = "UPDATE vaults SET is_default = 0";
            using var clearCommand = new SqliteCommand(clearSql, connection, transaction);
            await clearCommand.ExecuteNonQueryAsync(cancellationToken);

            // Set new default
            var setSql = "UPDATE vaults SET is_default = 1 WHERE id = @id";
            using var setCommand = new SqliteCommand(setSql, connection, transaction);
            setCommand.Parameters.AddWithValue("@id", vaultId.ToString());
            await setCommand.ExecuteNonQueryAsync(cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static VaultInfo ReadVaultInfo(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new VaultInfo(
            new VaultId(reader.GetString(0)), // id
            reader.GetString(1), // name
            reader.GetString(2), // path
            (byte[])reader[3],   // salt
            (byte[])reader[4],   // iv
            reader.GetString(5)  // version
        )
        {
            PasswordHint = reader.IsDBNull(6) ? null : reader.GetString(6),
            Icon = reader.IsDBNull(7) ? null : reader.GetString(7),
            CreatedAt = DateTime.Parse(reader.GetString(8)),
            ModifiedAt = DateTime.Parse(reader.GetString(9)),
            BiometricEnabled = reader.GetBoolean(10),
            BiometricKey = reader.IsDBNull(11) ? null : (byte[])reader[11]
        };
    }

    private static void AddVaultParameters(SqliteCommand command, VaultInfo vault)
    {
        command.Parameters.AddWithValue("@id", vault.Id.ToString());
        command.Parameters.AddWithValue("@name", vault.Name);
        command.Parameters.AddWithValue("@path", vault.Path);
        command.Parameters.AddWithValue("@salt", vault.Salt);
        command.Parameters.AddWithValue("@iv", vault.Iv);
        command.Parameters.AddWithValue("@version", vault.Version);
        command.Parameters.AddWithValue("@passwordHint", vault.PasswordHint ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@icon", vault.Icon ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", vault.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@modifiedAt", vault.ModifiedAt.ToString("O"));
        command.Parameters.AddWithValue("@biometricEnabled", vault.BiometricEnabled);
        command.Parameters.AddWithValue("@biometricKey", vault.BiometricKey ?? (object)DBNull.Value);
    }

    public void Dispose() { }
}
