namespace Erebus.Core;

/// <summary>
/// Represents a password credential for a service
/// </summary>
public sealed class SecurePasswordRecord : VaultRecord
{
    public SecurePasswordRecord(RecordId id, string title, string username, string password, string? url = null, string? otpSecret = null, DateTime createdAt = default)
        : base(id, RecordType.SecPass, title, createdAt == default ? DateTime.UtcNow : createdAt)
    {
        Username = username;
        Password = password;
        Url = url;
        OtpSecret = otpSecret;
    }

    public string Username { get; set; }
    public string Password { get; set; }
    public string? Url { get; set; }

    /// <summary>
    /// TOTP secret for 2FA (base32 encoded)
    /// </summary>
    public string? OtpSecret { get; set; }
    public string? DisplayName { get; set; }
}
