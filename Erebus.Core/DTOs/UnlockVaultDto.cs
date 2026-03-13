namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for unlocking a vault
/// </summary>
public sealed class UnlockVaultDto
{
    public VaultId VaultId { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool UseBiometric { get; set; }
}
