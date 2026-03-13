using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for migration operations
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Checks if migration is needed
    /// </summary>
    Task<bool> NeedsMigrationAsync(VaultId vaultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs migration
    /// </summary>
    Task MigrateAsync(VaultId vaultId, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}
