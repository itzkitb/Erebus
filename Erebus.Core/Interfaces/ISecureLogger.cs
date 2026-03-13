namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for logging with sensitive data filtering
/// </summary>
public interface ISecureLogger
{
    /// <summary>
    /// Logs an informational message
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Logs an error message
    /// </summary>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// Logs a debug message
    /// </summary>
    void LogDebug(string message);
}
