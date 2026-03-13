using Microsoft.Extensions.Logging;

namespace Erebus.Infrastructure.Logging;

/// <summary>
/// Logger provider that creates SecureLogger instances
/// </summary>
public sealed class SecureLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly List<SecureLogger> _loggers = new();
    private readonly object _lock = new();

    public SecureLoggerProvider(string logDirectory)
    {
        _logDirectory = logDirectory;
    }

    public ILogger CreateLogger(string categoryName)
    {
        lock (_lock)
        {
            var logger = new SecureLogger(_logDirectory, categoryName);
            _loggers.Add(logger);
            return logger;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (var logger in _loggers)
            {
                logger.Dispose();
            }
            _loggers.Clear();
        }
    }
}
