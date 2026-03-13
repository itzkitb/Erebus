using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for managing the file system operations
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Gets the base directory for Erebus data
    /// </summary>
    string GetBaseDirectory();

    /// <summary>
    /// Gets the path to the vaults database
    /// </summary>
    string GetVaultsDatabasePath();

    /// <summary>
    /// Gets the path to the settings database
    /// </summary>
    string GetSettingsDatabasePath();

    /// <summary>
    /// Gets the path to the logs directory
    /// </summary>
    string GetLogsDirectory();

    /// <summary>
    /// Gets the path to a specific vault directory
    /// </summary>
    string GetVaultDirectory(VaultId vaultId);

    /// <summary>
    /// Gets the path to the vault's index database
    /// </summary>
    string GetVaultIndexPath(VaultId vaultId);

    /// <summary>
    /// Gets the path to the vault's verify file
    /// </summary>
    string GetVaultVerifyPath(VaultId vaultId);

    /// <summary>
    /// Gets the path to the vault's tmp directory
    /// </summary>
    string GetVaultTmpDirectory(VaultId vaultId);

    /// <summary>
    /// Creates a directory if it doesn't exist
    /// </summary>
    Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file securely (overwrites with zeros first)
    /// </summary>
    Task SecureDeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all bytes from a file
    /// </summary>
    Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes all bytes to a file
    /// </summary>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Checks if a directory exists
    /// </summary>
    bool DirectoryExists(string path);
}
