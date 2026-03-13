namespace Erebus.Core;

/// <summary>
/// Represents the type of a secure record in the vault.
/// </summary>
public enum RecordType
{
    /// <summary>
    /// User-defined file
    /// </summary>
    SecFile = 0,

    /// <summary>
    /// Password credential
    /// </summary>
    SecPass = 1,

    /// <summary>
    /// Note
    /// </summary>
    SecNote = 2,

    /// <summary>
    /// Passport or ID document data
    /// </summary>
    SecPassport = 3
}