using Erebus.Core;
using Erebus.Core.Interfaces;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Implementation of vault migration service
/// </summary>
public sealed class MigrationService : IMigrationService
{
    private readonly IVaultMetadataRepository _metadataRepository;
    private readonly IFileSystem _fileSystem;
    private readonly ISecureLogger _logger;

    private static readonly string CurrentVersion = "1.0.0";

    public MigrationService(
        IVaultMetadataRepository metadataRepository,
        IFileSystem fileSystem,
        ISecureLogger logger)
    {
        _metadataRepository = metadataRepository;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<bool> NeedsMigrationAsync(VaultId vaultId, CancellationToken cancellationToken = default)
    {
        var vault = await _metadataRepository.GetVaultByIdAsync(vaultId, cancellationToken);
        if (vault == null)
        {
            _logger.LogWarning($"Vault {vaultId} not found for migration check");
            return false;
        }

        var needsMigration = vault.Version != CurrentVersion;

        if (needsMigration)
        {
            _logger.LogInfo($"Vault {vaultId} needs migration from {vault.Version} to {CurrentVersion}");
        }
        else
        {
            _logger.LogDebug($"Vault {vaultId} is up to date (version {vault.Version})");
        }

        return needsMigration;
    }

    public async Task MigrateAsync(VaultId vaultId, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var vault = await _metadataRepository.GetVaultByIdAsync(vaultId, cancellationToken);
        if (vault == null)
        {
            throw new ArgumentException($"Vault {vaultId} not found", nameof(vaultId));
        }

        progress?.Report($"Starting migration from {vault.Version} to {CurrentVersion}");
        _logger.LogInfo($"Starting migration for vault {vaultId} from {vault.Version} to {CurrentVersion}");

        // Version-by-version migration
        if (vault.Version == "1.0.0" && CurrentVersion == "1.0.0")
        {
            progress?.Report("No migration needed - vault is up to date");
            _logger.LogDebug("No migration needed");
            return;
        }

        // Future migrations example:
        // 1.0.0 -> 1.0.1: Add new fields to records
        // 1.0.1 -> 1.0.2: Change encryption algorithm
        // etc.

        // Update version
        vault.Version = CurrentVersion;
        await _metadataRepository.UpdateVaultAsync(vault, cancellationToken);

        progress?.Report("Migration completed successfully");
        _logger.LogInfo($"Migration completed for vault {vaultId}");
    }
}
