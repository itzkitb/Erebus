using Erebus.Core;

/// <summary>
/// Represents vault metadata stored in the main database
/// </summary>
public sealed class VaultInfo
{
    public VaultInfo(VaultId id, string name, string path, byte[] salt, byte[] iv, string version)
    {
        Id = id;
        Name = name;
        Path = path;
        Salt = salt;
        Iv = iv;
        Version = version;
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }

    public VaultId Id { get; }
    public string Name { get; set; }
    public string Path { get; set; }
    public byte[] Salt { get; set; }
    public byte[] Iv { get; set; }
    public string Version { get; set; }
    public string? PasswordHint { get; set; }

    /// <summary>
    /// Emoji icon for the vault
    /// </summary>
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool BiometricEnabled { get; set; }
    public byte[]? BiometricKey { get; set; }
}
