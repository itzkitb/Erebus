using Erebus.Core;
using Erebus.Core.Interfaces;
using Erebus.Infrastructure.FileSystem;
using Erebus.Infrastructure.Logging;
using Erebus.Infrastructure.Repositories;
using Erebus.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Erebus.Infrastructure;

/// <summary>
/// Composition root for dependency injection.
/// Registers all infrastructure services and repositories
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddErebusInfrastructure(this IServiceCollection services, string? baseDirectory = null)
    {
        // File System
        services.AddSingleton<IFileSystem>(sp =>
        {
            if (baseDirectory != null)
            {
                // For testing/custom directory
                return new OverrideFileSystem(baseDirectory);
            }
            return new PlatformFileSystem();
        });

        // Logging
        services.AddSingleton<ISecureLogger>(sp =>
        {
            var fileSystem = sp.GetRequiredService<IFileSystem>();
            return new SecureLogger(fileSystem.GetLogsDirectory());
        });

        // Repositories
        services.AddSingleton<IVaultMetadataRepository, VaultMetadataRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        // Services
        services.AddSingleton<IVaultService, VaultService>();
        services.AddSingleton<ISecureRandomGenerator, SecureRandomGenerator>();
        services.AddSingleton<ITimerService, TimerService>();
        services.AddSingleton<IBiometricService, BiometricService>();
        services.AddSingleton<IPasswordHealthService, PasswordHealthService>();
        services.AddSingleton<IExportImportService, ExportImportService>();
        services.AddSingleton<IMigrationService, MigrationService>();

        return services;
    }

    /// <summary>
    /// Creates the root composition root and returns a service provider
    /// </summary>
    public static IServiceProvider CreateCompositionRoot(string? baseDirectory = null)
    {
        var services = new ServiceCollection();
        services.AddErebusInfrastructure(baseDirectory);
        return services.BuildServiceProvider();
    }
}

/// <summary>
/// File system implementation with overridden base directory (for testing)
/// </summary>
internal sealed class OverrideFileSystem : IFileSystem
{
    private readonly string _baseDirectory;

    public OverrideFileSystem(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
        Directory.CreateDirectory(baseDirectory);
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

    public Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        return Task.CompletedTask;
    }

    public async Task SecureDeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return;

        var fileInfo = new FileInfo(path);
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None);
        var buffer = new byte[Math.Min(fileInfo.Length, 4096)];
        var remaining = fileInfo.Length;

        while (remaining > 0)
        {
            var toWrite = (int)Math.Min(buffer.Length, remaining);
            await stream.WriteAsync(buffer.AsMemory(0, toWrite), cancellationToken);
            remaining -= toWrite;
        }

        await stream.FlushAsync(cancellationToken);
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
            Directory.CreateDirectory(directory);

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(bytes.AsMemory(), cancellationToken);
    }
}
