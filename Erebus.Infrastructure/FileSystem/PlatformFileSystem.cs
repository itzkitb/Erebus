using Erebus.Core;
using Erebus.Core.Interfaces;

namespace Erebus.Infrastructure.FileSystem;

/// <summary>
/// Platform-aware file system service
/// </summary>
public sealed class PlatformFileSystem : IFileSystem
{
    private readonly string _baseDirectory;

    public PlatformFileSystem()
    {
        _baseDirectory = GetPlatformBaseDirectory();
    }

    public string GetBaseDirectory() => _baseDirectory;

    public string GetVaultsDatabasePath() => Path.Combine(_baseDirectory, "vaults.db");

    public string GetSettingsDatabasePath() => Path.Combine(_baseDirectory, "settings.db");

    public string GetLogsDirectory() => Path.Combine(_baseDirectory, "logs");

    public string GetVaultDirectory(VaultId vaultId) => Path.Combine(_baseDirectory, vaultId.ToString());

    public string GetVaultIndexPath(VaultId vaultId) => Path.Combine(GetVaultDirectory(vaultId), "index.db");

    public string GetVaultVerifyPath(VaultId vaultId) => Path.Combine(GetVaultDirectory(vaultId), "verify");

    public string GetVaultTmpDirectory(VaultId vaultId) => Path.Combine(GetVaultDirectory(vaultId), "tmp");

    public bool FileExists(string path) => File.Exists(path);

    public bool DirectoryExists(string path) => Directory.Exists(path);

    public async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            await Task.CompletedTask; // Placeholder
        }
    }

    public async Task SecureDeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return;

        var fileInfo = new FileInfo(path);
        var length = fileInfo.Length;

        // Overwrite with zeros
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None))
        {
            var buffer = new byte[Math.Min(length, 4096)];
            var remaining = length;
            
            while (remaining > 0)
            {
                var toWrite = (int)Math.Min(buffer.Length, remaining);
                await stream.WriteAsync(buffer.AsMemory(0, toWrite), cancellationToken);
                remaining -= toWrite;
            }
            
            await stream.FlushAsync(cancellationToken);
        }

        File.Delete(path);
    }

    public async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var buffer = new byte[stream.Length];
        await stream.ReadExactlyAsync(buffer.AsMemory(), cancellationToken);
        return buffer;
    }

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(bytes.AsMemory(), cancellationToken);
    }

    private static string GetPlatformBaseDirectory()
    {
        var os = Environment.OSVersion.Platform;
        var isWindows = OperatingSystem.IsWindows();
        var isLinux = OperatingSystem.IsLinux();
        var isAndroid = OperatingSystem.IsAndroid();

        if (isWindows)
        {
            // %AppData%/SillyApps/Erebus
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "SillyApps", "Erebus");
        }

        if (isLinux)
        {
            // ~/.local/share/SillyApps/Erebus
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, ".local", "share", "SillyApps", "Erebus");
        }

        if (isAndroid)
        {
            // /storage/emulated/0/Android/data/lol.tupid.erebus/files/
            // Use the appdata directory
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Erebus"
            );
        }

        // Fallback for macOS and etc
        // ~/Library/Application Support/SillyApps/Erebus
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, "Library", "Application Support", "SillyApps", "Erebus");
    }
}
