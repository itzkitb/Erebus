namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for managing inactivity timers
/// </summary>
public interface ITimerService
{
    /// <summary>
    /// Starts an inactivity timer
    /// </summary>
    void StartInactivityTimer(TimeSpan timeout, Action onTimeout);

    /// <summary>
    /// Resets the inactivity timer
    /// </summary>
    void ResetTimer();

    /// <summary>
    /// Stops and disposes the timer
    /// </summary>
    void StopTimer();
}
