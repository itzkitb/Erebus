using Erebus.Core;

namespace Erebus.Core.Interfaces;

/// <summary>
/// Repository for managing application settings
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets the current application settings
    /// </summary>
    Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the application settings
    /// </summary>
    Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
