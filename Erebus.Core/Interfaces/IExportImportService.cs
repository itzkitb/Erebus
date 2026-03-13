using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for export/import operations
/// </summary>
public interface IExportImportService
{
    /// <summary>
    /// Exports vault data to encrypted bytes
    /// </summary>
    Task<byte[]> ExportVaultAsync(IVaultRecordRepository repository, VaultInfo vaultInfo, string exportPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports vault data from encrypted bytes
    /// </summary>
    Task<VaultInfo> ImportVaultAsync(byte[] encryptedData, string importPassword, IVaultService vaultService, CancellationToken cancellationToken = default);
}
