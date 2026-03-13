namespace Erebus.Core;

/// <summary>
/// Represents the status of a vault
/// </summary>
public enum VaultStatus
{
    /// <summary>
    /// Vault is locked and requires password
    /// </summary>
    Locked = 0,

    /// <summary>
    /// Vault is unlocked and accessible
    /// </summary>
    Unlocked = 1,

    /// <summary>
    /// Vault is being migrated to a new format
    /// </summary>
    Migrating = 2,

    /// <summary>
    /// Vault is corrupted or inaccessible
    /// </summary>
    Corrupted = 3
}