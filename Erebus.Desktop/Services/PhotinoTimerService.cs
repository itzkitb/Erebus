using System.Timers;
using Erebus.Core.Interfaces;

namespace Erebus.Desktop.Services;

/// <summary>
/// Photino implementation of timer service.
/// </summary>
public sealed class PhotinoTimerService : ITimerService, IDisposable
{
    private System.Timers.Timer? _timer;
    private Action? _onTimeout;
    private bool _disposed;
    
    public void StartInactivityTimer(TimeSpan timeout, Action onTimeout)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PhotinoTimerService));
        StopTimer();
        
        _onTimeout = onTimeout;
        _timer = new System.Timers.Timer(timeout.TotalMilliseconds) { AutoReset = false };
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }
    
    public void ResetTimer()
    {
        if (_timer != null && _timer.Enabled)
        {
            _timer.Stop();
            _timer.Start();
        }
    }
    
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
