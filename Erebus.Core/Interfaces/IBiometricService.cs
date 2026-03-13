namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for biometric authentication
/// </summary>
public interface IBiometricService
{
    /// <summary>
    /// Checks if biometric authentication is available
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates using biometrics
    /// </summary>
    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);
}
