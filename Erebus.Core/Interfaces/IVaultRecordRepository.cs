using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Repository for managing secure records within a vault
/// </summary>
public interface IVaultRecordRepository : IDisposable
{
    /// <summary>
    /// Gets all records in the vault
    /// </summary>
    Task<IReadOnlyList<VaultRecord>> GetAllRecordsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a record by its ID
    /// </summary>
    Task<VaultRecord?> GetRecordByIdAsync(RecordId recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets records by type
    /// </summary>
    Task<IReadOnlyList<VaultRecord>> GetRecordsByTypeAsync(RecordType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets records in a specific folder
    /// </summary>
    Task<IReadOnlyList<VaultRecord>> GetRecordsByFolderAsync(string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches records by title or tags
    /// </summary>
    Task<IReadOnlyList<VaultRecord>> SearchRecordsAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new record
    /// </summary>
    Task<VaultRecord> CreateRecordAsync(VaultRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing record
    /// </summary>
    Task UpdateRecordAsync(VaultRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a record
    /// </summary>
    Task DeleteRecordAsync(RecordId recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the encrypted content for a file record
    /// </summary>
    Task<byte[]> GetFileContentAsync(RecordId recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the encrypted content for a file record
    /// </summary>
    Task SetFileContentAsync(RecordId recordId, byte[] encryptedContent, CancellationToken cancellationToken = default);
}
