namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for creating a new vault
/// </summary>
public sealed class CreateVaultDto
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PasswordHint { get; set; }
    public string? Icon { get; set; }
    public bool EnableBackups { get; set; }
    public bool EnableBiometric { get; set; }
}
