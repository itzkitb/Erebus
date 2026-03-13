using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for managing vault lifecycle operations
/// </summary>
public interface IVaultService
{
    /// <summary>
    /// Gets all vaults available to the user
    /// </summary>
    Task<IReadOnlyList<VaultInfo>> GetAllVaultsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new vault
    /// </summary>
    Task<VaultInfo> CreateVaultAsync(string name, string password, string? passwordHint = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a vault and returns a record repository
    /// </summary>
    Task<IVaultRecordRepository> OpenVaultAsync(VaultId vaultId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the password for a vault
    /// </summary>
    Task<bool> VerifyPasswordAsync(VaultId vaultId, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a vault
    /// </summary>
    Task CloseVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a vault
    /// </summary>
    Task<VaultStatus> GetVaultStatusAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Migrates a vault to the latest version
    /// </summary>
    Task MigrateVaultAsync(VaultId vaultId, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a backup of the vault
    /// </summary>
    Task<string> BackupVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports vault data to encrypted bytes
    /// </summary>
    Task<byte[]> ExportVaultAsync(VaultId vaultId, string exportPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports vault data from encrypted bytes
    /// </summary>
    Task<VaultInfo> ImportVaultAsync(byte[] encryptedData, string importPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vault permanently
    /// </summary>
    Task DeleteVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default);
}
