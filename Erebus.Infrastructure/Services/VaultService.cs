using Erebus.Core;
using Erebus.Core.Interfaces;
using Erebus.Cryptography;
using Erebus.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Main service for managing vault lifecycle operations
/// </summary>
public sealed class VaultService : IVaultService
{
    private readonly IVaultMetadataRepository _vaultMetadataRepository;
    private readonly IFileSystem _fileSystem;
    private readonly IExportImportService _exportImportService;
    private readonly Dictionary<VaultId, VaultCryptoService> _openVaults = new();

    public VaultService(
        IVaultMetadataRepository vaultMetadataRepository,
        IFileSystem fileSystem,
        IExportImportService exportImportService)
    {
        _vaultMetadataRepository = vaultMetadataRepository;
        _fileSystem = fileSystem;
        _exportImportService = exportImportService;
    }

    public async Task<IReadOnlyList<VaultInfo>> GetAllVaultsAsync(CancellationToken cancellationToken = default)
    {
        return await _vaultMetadataRepository.GetAllVaultsAsync(cancellationToken);
    }

    public async Task<VaultInfo> CreateVaultAsync(string name, string password, string? passwordHint = null, CancellationToken cancellationToken = default)
    {
        var vaultId = VaultId.CreateNew();
        var vaultDir = _fileSystem.GetVaultDirectory(vaultId);
        
        // Create vault
        await _fileSystem.CreateDirectoryAsync(vaultDir, cancellationToken);
        await _fileSystem.CreateDirectoryAsync(Path.Combine(vaultDir, "vault"), cancellationToken);
        await _fileSystem.CreateDirectoryAsync(Path.Combine(vaultDir, "tmp"), cancellationToken);

        // Generate salt and IV
        var salt = Argon2Hasher.GenerateSalt();
        var iv = AesGcmCipher.GenerateKey(); // Using as unique identifier

        // Create verify hash
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var verifySalt = Argon2Hasher.GenerateSalt();
        var verifyHash = VaultCryptoService.CreateVerificationHash(passwordBytes, verifySalt);

        var verifyPath = _fileSystem.GetVaultVerifyPath(vaultId);
        var verifyData = new byte[verifySalt.Length + verifyHash.Length];
        Buffer.BlockCopy(verifySalt, 0, verifyData, 0, verifySalt.Length);
        Buffer.BlockCopy(verifyHash, 0, verifyData, verifySalt.Length, verifyHash.Length);
        await _fileSystem.WriteAllBytesAsync(verifyPath, verifyData, cancellationToken);

        // Create vault meta
        var vault = new VaultInfo(vaultId, name, vaultDir, salt, iv, "1.0.0")
        {
            PasswordHint = passwordHint
        };

        await _vaultMetadataRepository.CreateVaultAsync(vault, cancellationToken);

        // Create index database with SQLCipher
        var indexPath = _fileSystem.GetVaultIndexPath(vaultId);
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = indexPath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Password = password
        }.ToString();

        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Create tables
        var createTableSql = """
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

        using var command = new Microsoft.Data.Sqlite.SqliteCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return vault;
    }

    public async Task<IVaultRecordRepository> OpenVaultAsync(VaultId vaultId, string password, CancellationToken cancellationToken = default)
    {
        if (_openVaults.ContainsKey(vaultId))
        {
            throw new InvalidOperationException("Vault is already open");
        }

        var vault = await _vaultMetadataRepository.GetVaultByIdAsync(vaultId, cancellationToken)
            ?? throw new ArgumentException("Vault not found", nameof(vaultId));

        // Verify password
        var isValid = await VerifyPasswordAsync(vaultId, password, cancellationToken);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        // Derive encryption key from password using HKDF
        var encryptionKey = VaultRecordRepository.DeriveKey(password, vault.Salt);

        // Initialize crypto service
        var cryptoService = new VaultCryptoService();
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        cryptoService.InitializeFromPassword(passwordBytes, vault.Salt);
        _openVaults[vaultId] = cryptoService;

        // Return repository with derived key
        return new VaultRecordRepository(vaultId, _fileSystem, encryptionKey);
    }

    public async Task<bool> VerifyPasswordAsync(VaultId vaultId, string password, CancellationToken cancellationToken = default)
    {
        var verifyPath = _fileSystem.GetVaultVerifyPath(vaultId);
        if (!_fileSystem.FileExists(verifyPath))
            return false;

        var verifyData = await _fileSystem.ReadAllBytesAsync(verifyPath, cancellationToken);
        var saltLength = CryptoConstants.Argon2SaltSizeBytes;
        
        var verifySalt = new byte[saltLength];
        var storedHash = new byte[verifyData.Length - saltLength];
        
        Buffer.BlockCopy(verifyData, 0, verifySalt, 0, saltLength);
        Buffer.BlockCopy(verifyData, saltLength, storedHash, 0, verifyData.Length - saltLength);

        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        return VaultCryptoService.VerifyPassword(passwordBytes, verifySalt, storedHash);
    }

    public Task CloseVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        if (_openVaults.TryGetValue(vaultId, out var cryptoService))
        {
            cryptoService.Dispose();
            _openVaults.Remove(vaultId);
        }
        return Task.CompletedTask;
    }

    public async Task<VaultStatus> GetVaultStatusAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        var vault = await _vaultMetadataRepository.GetVaultByIdAsync(vaultId, cancellationToken);
        if (vault == null)
            return VaultStatus.Corrupted;

        if (_openVaults.ContainsKey(vaultId))
            return VaultStatus.Unlocked;

        return VaultStatus.Locked;
    }

    public Task MigrateVaultAsync(VaultId vaultId, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        // TODO: Implement migration logic for future versions
        progress?.Report("Vault is up to date");
        return Task.CompletedTask;
    }

    public async Task<string> BackupVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        var vault = await _vaultMetadataRepository.GetVaultByIdAsync(vaultId, cancellationToken)
            ?? throw new ArgumentException("Vault not found", nameof(vaultId));

        var backupDir = Path.Combine(_fileSystem.GetBaseDirectory(), "backups");
        await _fileSystem.CreateDirectoryAsync(backupDir, cancellationToken);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(backupDir, $"backup_{vault.Name}_{timestamp}");

        var vaultDir = _fileSystem.GetVaultDirectory(vaultId);
        if (_fileSystem.DirectoryExists(vaultDir))
        {
            await CopyDirectoryAsync(vaultDir, backupPath, cancellationToken);
        }

        return backupPath;
    }

    private async Task CopyDirectoryAsync(string source, string destination, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(source))
            return;

        Directory.CreateDirectory(destination);

        foreach (var file in Directory.GetFiles(source))
        {
            var destFile = Path.Combine(destination, Path.GetFileName(file));
            var bytes = await _fileSystem.ReadAllBytesAsync(file, cancellationToken);
            await _fileSystem.WriteAllBytesAsync(destFile, bytes, cancellationToken);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            var destDir = Path.Combine(destination, Path.GetFileName(dir));
            await CopyDirectoryAsync(dir, destDir, cancellationToken);
        }
    }

    public async Task<byte[]> ExportVaultAsync(VaultId vaultId, string exportPassword, CancellationToken cancellationToken = default)
    {
        var vault = await _vaultMetadataRepository.GetVaultByIdAsync(vaultId, cancellationToken)
            ?? throw new ArgumentException("Vault not found", nameof(vaultId));

        var repository = await OpenVaultAsync(vaultId, exportPassword, cancellationToken);
        return await _exportImportService.ExportVaultAsync(repository, vault, exportPassword, cancellationToken);
    }

    public async Task<VaultInfo> ImportVaultAsync(byte[] encryptedData, string importPassword, CancellationToken cancellationToken = default)
    {
        return await _exportImportService.ImportVaultAsync(encryptedData, importPassword, this, cancellationToken);
    }

    public async Task DeleteVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        // Close if open
        await CloseVaultAsync(vaultId, cancellationToken);

        // Delete vault directory
        var vaultDir = _fileSystem.GetVaultDirectory(vaultId);
        if (_fileSystem.DirectoryExists(vaultDir))
        {
            // Securely delete all files
            foreach (var file in Directory.GetFiles(vaultDir, "*", SearchOption.AllDirectories))
            {
                await _fileSystem.SecureDeleteFileAsync(file, cancellationToken);
            }
            Directory.Delete(vaultDir, true);
        }

        // Delete meta
        await _vaultMetadataRepository.DeleteVaultAsync(vaultId, cancellationToken);
    }
}
