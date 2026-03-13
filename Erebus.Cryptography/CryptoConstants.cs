namespace Erebus.Cryptography;

/// <summary>
/// Cryptographic constants for the Erebus password manager.
/// </summary>
public static class CryptoConstants
{
    /// <summary>
    /// AES-256 key size in bytes (256 bits).
    /// </summary>
    public const int AesKeySizeBytes = 32;

    /// <summary>
    /// AES-GCM nonce size in bytes (96 bits).
    /// </summary>
    public const int AesNonceSizeBytes = 12;

    /// <summary>
    /// AES-GCM tag size in bytes (128 bits).
    /// </summary>
    public const int AesTagSizeBytes = 16;

    /// <summary>
    /// Argon2id salt size in bytes.
    /// </summary>
    public const int Argon2SaltSizeBytes = 16;

    /// <summary>
    /// Argon2id memory cost in KiB (64 MB).
    /// </summary>
    public const int Argon2MemoryCostKib = 65536;

    /// <summary>
    /// Argon2id iterations (parallelism).
    /// </summary>
    public const int Argon2Parallelism = 4;

    /// <summary>
    /// Argon2id time cost (iterations).
    /// </summary>
    public const int Argon2TimeCost = 3;

    /// <summary>
    /// HKDF SHA-256 output size for key derivation.
    /// </summary>
    public const int HkdfOutputSizeBytes = 32;
}
