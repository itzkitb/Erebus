namespace Erebus.Core;

/// <summary>
/// Result of password strength analysis
/// </summary>
public sealed class PasswordStrengthResult
{
    public int Score { get; set; }
    public bool HasUppercase { get; set; }
    public bool HasLowercase { get; set; }
    public bool HasDigits { get; set; }
    public bool HasSymbols { get; set; }
    public int Length { get; set; }
    public string Feedback { get; set; } = "";
}
