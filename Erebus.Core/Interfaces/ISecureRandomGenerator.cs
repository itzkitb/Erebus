namespace Erebus.Core.Interfaces;

/// <summary>
/// Service for secure random generation
/// </summary>
public interface ISecureRandomGenerator
{
    /// <summary>
    /// Generates a secure password
    /// </summary>
    string GeneratePassword(int length, bool includeUppercase, bool includeDigits, bool includeSymbols, bool includeAmbiguous);

    /// <summary>
    /// Generates random bytes
    /// </summary>
    byte[] GenerateRandomBytes(int count);
}
