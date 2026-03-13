using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Repository for managing vault metadata
/// </summary>
public interface IVaultMetadataRepository
{
    /// <summary>
    /// Gets all vaults available to the user
    /// </summary>
    Task<IReadOnlyList<VaultInfo>> GetAllVaultsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a vault by its ID
    /// </summary>
    Task<VaultInfo?> GetVaultByIdAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new vault entry
    /// </summary>
    Task<VaultInfo> CreateVaultAsync(VaultInfo vault, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing vault entry
    /// </summary>
    Task UpdateVaultAsync(VaultInfo vault, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vault entry (does not delete files)
    /// </summary>
    Task DeleteVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default/last opened vault
    /// </summary>
    Task<VaultInfo?> GetDefaultVaultAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the default vault
    /// </summary>
    Task SetDefaultVaultAsync(VaultId vaultId, CancellationToken cancellationToken = default);
}
