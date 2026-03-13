using Erebus.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Erebus.Infrastructure.Logging;

/// <summary>
/// Secure logger that filters sensitive data before writing
/// Implements both ISecureLogger and ILogger interfaces
/// </summary>
public sealed class SecureLogger : ISecureLogger, ILogger, IDisposable
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly string _categoryName;
    private readonly object _lock = new();
    private bool _disposed;

    // Patterns that might indicate sensitive data (TODO: Need update, just example rn)
    private static readonly string[] SensitivePatterns = { "password", "passwd", "secret", "key", "token", "credential" };
    private const string RedactedText = "***REDACTED***";

    public SecureLogger(string logDirectory)
        : this(logDirectory, "Erebus")
    {
    }

    public SecureLogger(string logDirectory, string categoryName)
    {
        _logDirectory = logDirectory;
        _categoryName = categoryName;
        _logFilePath = Path.Combine(logDirectory, $"erebus_{DateTime.UtcNow:yyyyMMdd}.log");

        Directory.CreateDirectory(logDirectory);
        CleanupOldLogs();
    }

    public void LogInfo(string message)
    {
        if (_disposed) return;
        Log("INFO", message);
    }

    public void LogWarning(string message)
    {
        if (_disposed) return;
        Log("WARN", message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        if (_disposed) return;
        var fullMessage = exception != null
            ? $"{message}{Environment.NewLine}Exception: {exception}"
            : message;
        Log("ERROR", fullMessage);
    }

    public void LogDebug(string message)
    {
        if (_disposed) return;
        Log("DEBUG", message);
    }

    private void Log(string level, string message)
    {
        try
        {
            var sanitizedMessage = SanitizeMessage(message);
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logLine = $"[{timestamp}] [{level}] [{_categoryName}] {sanitizedMessage}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logLine);
            }
        }
        catch
        {
            // Just in case
        }
    }

    private static string SanitizeMessage(string message)
    {
        var sanitized = message;

        // Check for potential sensitive data patterns
        foreach (var pattern in SensitivePatterns)
        {
            if (sanitized.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                sanitized = System.Text.RegularExpressions.Regex.Replace(
                    sanitized,
                    $@"(?<={pattern}\s*[=:]\s*).+?(?=\s|$|,|;)",
                    RedactedText,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }
        }

        // Redact long base64 strings (~keys/tokens)
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"[A-Za-z0-9+/]{50,}={0,2}",
            RedactedText
        );

        // Redact GUIDs
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"\b[a-fA-F0-9]{32}\b",
            RedactedText
        );

        return sanitized;
    }

    private void CleanupOldLogs()
    {
        try
        {
            var logFiles = Directory.GetFiles(_logDirectory, "erebus_*.log");
            var cutoffDate = DateTime.UtcNow.AddDays(-7);

            foreach (var logFile in logFiles)
            {
                var fileName = Path.GetFileName(logFile);
                if (TryParseLogDate(fileName, out var logDate) && logDate < cutoffDate)
                {
                    File.Delete(logFile);
                }
            }
        }
        catch
        {
            // Whats up?
        }
    }

    private static bool TryParseLogDate(string fileName, out DateTime date)
    {
        // erebus_YYYYMMDD.log
        if (fileName.StartsWith("erebus_") && fileName.EndsWith(".log") && fileName.Length == 18)
        {
            var datePart = fileName.Substring(7, 8);
            return DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date);
        }

        date = DateTime.MinValue;
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }

    // ILogger interface implementation
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var level = logLevel switch
        {
            LogLevel.Critical => "FATAL",
            LogLevel.Error => "ERROR",
            LogLevel.Warning => "WARN",
            LogLevel.Information => "INFO",
            LogLevel.Debug => "DEBUG",
            LogLevel.Trace => "TRACE",
            _ => "INFO"
        };

        if (exception != null)
        {
            message += $"{Environment.NewLine}Exception: {exception}";
        }

        Log(level, message);
    }
}
