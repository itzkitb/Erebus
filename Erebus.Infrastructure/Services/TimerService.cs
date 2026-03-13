using System.Timers;
using Erebus.Core.Interfaces;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Service for managing inactivity timers
/// </summary>
public sealed class TimerService : ITimerService, IDisposable
{
    private System.Timers.Timer? _timer;
    private Action? _onTimeout;
    private bool _disposed;

    /// <summary>
    /// Starts an inactivity timer
    /// </summary>
    /// <param name="timeout">Timeout duration</param>
    /// <param name="onTimeout">Action on timeout</param>
    public void StartInactivityTimer(TimeSpan timeout, Action onTimeout)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TimerService));

        StopTimer();

        _onTimeout = onTimeout;
        _timer = new System.Timers.Timer(timeout.TotalMilliseconds)
        {
            AutoReset = false
        };
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    /// <summary>
    /// Resets the inactivity timer
    /// </summary>
    public void ResetTimer()
    {
        if (_timer != null && _timer.Enabled)
        {
            _timer.Stop();
            _timer.Start();
        }
    }

    /// <summary>
    /// Stops and disposes the timer
    /// </summary>
    public void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Elapsed -= OnTimerElapsed;
            _timer.Dispose();
            _timer = null;
        }
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _onTimeout?.Invoke();
    }

    public void Dispose()
    {
        if (_disposed) return;
        StopTimer();
        _disposed = true;
    }
}
