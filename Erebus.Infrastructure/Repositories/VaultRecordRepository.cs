using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Erebus.Core;
using Erebus.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace Erebus.Infrastructure.Repositories;

/// <summary>
/// SQLCipher implementation of the vault record repository.
/// Stores encrypted records within a specific vault
/// </summary>
public sealed class VaultRecordRepository : IVaultRecordRepository
{
    private readonly string _connectionString;
    private readonly VaultId _vaultId;
    private readonly IFileSystem _fileSystem;
    private bool _disposed;

    public VaultRecordRepository(VaultId vaultId, IFileSystem fileSystem, byte[] encryptionKey)
    {
        _vaultId = vaultId;
        _fileSystem = fileSystem;

        var dbPath = fileSystem.GetVaultIndexPath(vaultId);
        
        // Convert encryption key to hex string for SQLCipher
        var hexKey = BitConverter.ToString(encryptionKey).Replace("-", "");
        
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWrite,
            Password = $"hexkey:{hexKey}"
        };
        _connectionString = builder.ToString();

        InitializeDatabase();
    }

    /// <summary>
    /// Derives encryption key from master password using HKDF-SHA256.
    /// </summary>
    public static byte[] DeriveKey(string password, byte[] salt, int keyLength = 32)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        
        // HKDF-SHA256
        // Step 0: Extract
        using var hmac = new System.Security.Cryptography.HMACSHA256(salt);
        var prk = hmac.ComputeHash(passwordBytes);
        
        // Step 1: Expand
        var output = new byte[keyLength];
        var iterations = (keyLength + 32 - 1) / 32; // Ceiling division
        
        var previousHash = Array.Empty<byte>();
        var blockIndex = 1;
        
        for (var i = 0; i < iterations; i++)
        {
            using var expandHmac = new System.Security.Cryptography.HMACSHA256(prk);
            var data = new byte[previousHash.Length + 1];
            Buffer.BlockCopy(previousHash, 0, data, 0, previousHash.Length);
            data[^1] = (byte)blockIndex;
            
            var hash = expandHmac.ComputeHash(data);
            previousHash = hash;
            
            var copyLength = Math.Min(32, keyLength - i * 32);
            Buffer.BlockCopy(hash, 0, output, i * 32, copyLength);
        }
        
        return output;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Enable WAL mode for performance thngs
        using var walCommand = new SqliteCommand("PRAGMA journal_mode = WAL;", connection);
        walCommand.ExecuteNonQuery();

        var createRecordsTableSql = """
            CREATE TABLE IF NOT EXISTS records (
                id TEXT PRIMARY KEY,
                type INTEGER NOT NULL,
                title TEXT NOT NULL,
                folder TEXT,
                tags TEXT,
                created_at TEXT NOT NULL,
                modified_at TEXT NOT NULL,
                data TEXT NOT NULL
            )
            """;

        using var command = new SqliteCommand(createRecordsTableSql, connection);
        command.ExecuteNonQuery();
    }

    public async Task<IReadOnlyList<VaultRecord>> GetAllRecordsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM records ORDER BY modified_at DESC";
        var records = new List<VaultRecord>();

        using var command = new SqliteCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = ReadRecord(reader);
            if (record != null)
                records.Add(record);
        }

        return records;
    }

    public async Task<VaultRecord?> GetRecordByIdAsync(RecordId recordId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM records WHERE id = @id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", recordId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return ReadRecord(reader);
        }

        return null;
    }

    public async Task<IReadOnlyList<VaultRecord>> GetRecordsByTypeAsync(RecordType type, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM records WHERE type = @type ORDER BY title";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@type", (int)type);

        var records = new List<VaultRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = ReadRecord(reader);
            if (record != null)
                records.Add(record);
        }

        return records;
    }

    public async Task<IReadOnlyList<VaultRecord>> GetRecordsByFolderAsync(string folder, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = "SELECT * FROM records WHERE folder = @folder ORDER BY title";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@folder", folder);

        var records = new List<VaultRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = ReadRecord(reader);
            if (record != null)
                records.Add(record);
        }

        return records;
    }

    public async Task<IReadOnlyList<VaultRecord>> SearchRecordsAsync(string query, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            SELECT * FROM records 
            WHERE title LIKE @query OR tags LIKE @query
            ORDER BY modified_at DESC
            """;
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@query", $"%{query}%");

        var records = new List<VaultRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var record = ReadRecord(reader);
            if (record != null)
                records.Add(record);
        }

        return records;
    }

    public async Task<VaultRecord> CreateRecordAsync(VaultRecord record, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            INSERT INTO records (id, type, title, folder, tags, created_at, modified_at, data)
            VALUES (@id, @type, @title, @folder, @tags, @createdAt, @modifiedAt, @data)
            """;

        using var command = new SqliteCommand(sql, connection);
        AddRecordParameters(command, record);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return record;
    }

    public async Task UpdateRecordAsync(VaultRecord record, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = """
            UPDATE records 
            SET title = @title, folder = @folder, tags = @tags, modified_at = @modifiedAt, data = @data
            WHERE id = @id
            """;

        using var command = new SqliteCommand(sql, connection);
        AddRecordParameters(command, record);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteRecordAsync(RecordId recordId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // First, get the record to check if its a file
        var record = await GetRecordByIdAsync(recordId, cancellationToken);
        if (record is SecureFileRecord fileRecord)
        {
            // Securely delete the file content
            var filePath = Path.Combine(_fileSystem.GetVaultDirectory(_vaultId), "vault", fileRecord.EncryptedFilePath);
            if (File.Exists(filePath))
            {
                await _fileSystem.SecureDeleteFileAsync(filePath, cancellationToken);
            }
        }

        var sql = "DELETE FROM records WHERE id = @id";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@id", recordId.ToString());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<byte[]> GetFileContentAsync(RecordId recordId, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var filePath = Path.Combine(_fileSystem.GetVaultDirectory(_vaultId), "vault", recordId.ToString());
        return await _fileSystem.ReadAllBytesAsync(filePath, cancellationToken);
    }

    public async Task SetFileContentAsync(RecordId recordId, byte[] encryptedContent, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var vaultDir = _fileSystem.GetVaultDirectory(_vaultId);
        var vaultFilesDir = Path.Combine(vaultDir, "vault");
        
        await _fileSystem.CreateDirectoryAsync(vaultFilesDir, cancellationToken);
        
        var filePath = Path.Combine(vaultFilesDir, recordId.ToString());
        await _fileSystem.WriteAllBytesAsync(filePath, encryptedContent, cancellationToken);
    }

    private VaultRecord? ReadRecord(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        var id = new RecordId(reader.GetString(0));
        var type = (RecordType)reader.GetInt32(1);
        var title = reader.GetString(2);
        var folder = reader.IsDBNull(3) ? null : reader.GetString(3);
        var tagsJson = reader.IsDBNull(4) ? "[]" : reader.GetString(4);
        var createdAt = DateTime.Parse(reader.GetString(5));
        var modifiedAt = DateTime.Parse(reader.GetString(6));
        var dataJson = reader.GetString(7);

        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();

        return type switch
        {
            RecordType.SecFile => ReadSecureFile(id, title, folder, tags, createdAt, modifiedAt, dataJson),
            RecordType.SecPass => ReadSecurePassword(id, title, folder, tags, createdAt, modifiedAt, dataJson),
            RecordType.SecNote => ReadSecureNote(id, title, folder, tags, createdAt, modifiedAt, dataJson),
            RecordType.SecPassport => ReadSecurePassport(id, title, folder, tags, createdAt, modifiedAt, dataJson),
            _ => null
        };
    }

    private static SecureFileRecord ReadSecureFile(RecordId id, string title, string? folder, List<string> tags, DateTime createdAt, DateTime modifiedAt, string dataJson)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(dataJson);
        var record = new SecureFileRecord(
            id, title,
            data.GetProperty("fileName").GetString() ?? "",
            data.GetProperty("fileSize").GetInt64(),
            data.GetProperty("mimeType").GetString() ?? "",
            createdAt
        )
        {
            Folder = folder,
            EncryptedFilePath = data.GetProperty("encryptedPath").GetString() ?? ""
        };
        record.Tags.AddRange(tags);
        // Use reflection to set ModifiedAt since its protected
        typeof(VaultRecord).GetProperty("ModifiedAt")?.SetValue(record, modifiedAt);
        return record;
    }

    private static SecurePasswordRecord ReadSecurePassword(RecordId id, string title, string? folder, List<string> tags, DateTime createdAt, DateTime modifiedAt, string dataJson)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(dataJson);
        var record = new SecurePasswordRecord(
            id, title,
            data.GetProperty("u").GetString() ?? "",
            data.GetProperty("p").GetString() ?? "",
            data.TryGetProperty("url", out var url) ? url.GetString() : null,
            data.TryGetProperty("otp", out var otp) ? otp.GetString() : null,
            createdAt
        )
        {
            Folder = folder,
            DisplayName = data.TryGetProperty("n", out var n) ? n.GetString() : null
        };
        record.Tags.AddRange(tags);
        typeof(VaultRecord).GetProperty("ModifiedAt")?.SetValue(record, modifiedAt);
        return record;
    }

    private static SecureNoteRecord ReadSecureNote(RecordId id, string title, string? folder, List<string> tags, DateTime createdAt, DateTime modifiedAt, string dataJson)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(dataJson);
        var record = new SecureNoteRecord(
            id, title,
            data.GetProperty("content").GetString() ?? "",
            data.TryGetProperty("fmt", out var fmt) ? fmt.GetString() : "md",
            createdAt
        )
        {
            Folder = folder
        };
        record.Tags.AddRange(tags);
        typeof(VaultRecord).GetProperty("ModifiedAt")?.SetValue(record, modifiedAt);
        return record;
    }

    private static SecurePassportRecord ReadSecurePassport(RecordId id, string title, string? folder, List<string> tags, DateTime createdAt, DateTime modifiedAt, string dataJson)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(dataJson);
        var record = new SecurePassportRecord(
            id, title,
            data.GetProperty("fn").GetString() ?? "",
            data.GetProperty("num").GetString() ?? "",
            DateTime.Parse(data.GetProperty("dob").GetString() ?? DateTime.MinValue.ToString()),
            DateTime.Parse(data.GetProperty("iss").GetString() ?? DateTime.MinValue.ToString()),
            DateTime.Parse(data.GetProperty("exp").GetString() ?? DateTime.MaxValue.ToString()),
            data.GetProperty("auth").GetString() ?? "",
            data.TryGetProperty("code", out var code) ? code.GetString() : null,
            data.TryGetProperty("c", out var c) ? c.GetString() : "RUS",
            createdAt
        )
        {
            Folder = folder
        };
        record.Tags.AddRange(tags);
        typeof(VaultRecord).GetProperty("ModifiedAt")?.SetValue(record, modifiedAt);
        return record;
    }

    private static void AddRecordParameters(SqliteCommand command, VaultRecord record)
    {
        command.Parameters.AddWithValue("@id", record.Id.ToString());
        command.Parameters.AddWithValue("@type", (int)record.Type);
        command.Parameters.AddWithValue("@title", record.Title);
        command.Parameters.AddWithValue("@folder", record.Folder ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@tags", JsonSerializer.Serialize(record.Tags));
        command.Parameters.AddWithValue("@createdAt", record.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@modifiedAt", record.ModifiedAt.ToString("O"));
        command.Parameters.AddWithValue("@data", SerializeRecordData(record));
    }

    private static string SerializeRecordData(VaultRecord record)
    {
        return record switch
        {
            SecureFileRecord f => JsonSerializer.Serialize(new
            {
                fileName = f.FileName,
                fileSize = f.FileSize,
                mimeType = f.MimeType,
                encryptedPath = f.EncryptedFilePath
            }),
            SecurePasswordRecord p => JsonSerializer.Serialize(new
            {
                u = p.Username,
                p = p.Password,
                url = p.Url,
                otp = p.OtpSecret,
                n = p.DisplayName
            }),
            SecureNoteRecord n => JsonSerializer.Serialize(new
            {
                fmt = n.Format,
                content = n.Content
            }),
            SecurePassportRecord p => JsonSerializer.Serialize(new
            {
                fn = p.FullName,
                num = p.DocumentNumber,
                dob = p.DateOfBirth.ToString("yyyy-MM-dd"),
                iss = p.IssueDate.ToString("yyyy-MM-dd"),
                exp = p.ExpiryDate.ToString("yyyy-MM-dd"),
                auth = p.IssuingAuthority,
                code = p.SubdivisionCode,
                c = p.Country
            }),
            _ => "{}"
        };
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(VaultRecordRepository));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
