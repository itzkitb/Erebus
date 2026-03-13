namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for password health analysis
/// </summary>
public interface IPasswordHealthService
{
    /// <summary>
    /// Analyzes password strength
    /// </summary>
    PasswordStrengthResult AnalyzePassword(string password);

    /// <summary>
    /// Checks for compromised passwords
    /// </summary>
    Task<bool> IsPasswordCompromisedAsync(string password, CancellationToken cancellationToken = default);
}
