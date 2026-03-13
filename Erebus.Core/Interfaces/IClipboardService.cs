namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for clipboard operations
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Copies text to the system clipboard
    /// </summary>
    Task CopyAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets text from the system clipboard
    /// </summary>
    Task<string?> GetTextAsync(CancellationToken cancellationToken = default);
}
