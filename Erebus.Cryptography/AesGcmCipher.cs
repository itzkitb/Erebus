using System.Security.Cryptography;

namespace Erebus.Cryptography;

/// <summary>
/// Provides AES-256-GCM encryption and decryption functionality.
/// </summary>
public static class AesGcmCipher
{
    /// <summary>
    /// Encrypts data using AES-256-GCM.
    /// </summary>
    /// <param name="key">The encryption key (must be 32 bytes for AES-256).</param>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="associatedData">Optional additional authenticated data (can be empty).</param>
    /// <returns>
    /// A tuple containing:
    /// - Nonce (12 bytes)
    /// - Ciphertext (encrypted data)
    /// - Tag (16 bytes authentication tag)
    /// </returns>
    public static (byte[] Nonce, byte[] Ciphertext, byte[] Tag) Encrypt(
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> associatedData = default)
    {
        if (key.Length != CryptoConstants.AesKeySizeBytes)
            throw new ArgumentException($"Key must be {CryptoConstants.AesKeySizeBytes} bytes for AES-256", nameof(key));

        var nonce = new byte[CryptoConstants.AesNonceSizeBytes];
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[CryptoConstants.AesTagSizeBytes];

        using var aesGcm = new System.Security.Cryptography.AesGcm(key, CryptoConstants.AesTagSizeBytes);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

        return (nonce, ciphertext, tag);
    }

    /// <summary>
    /// Decrypts data using AES-256-GCM.
    /// </summary>
    /// <param name="key">The encryption key (must be 32 bytes for AES-256).</param>
    /// <param name="nonce">The nonce used during encryption (12 bytes).</param>
    /// <param name="ciphertext">The encrypted data.</param>
    /// <param name="tag">The authentication tag (16 bytes).</param>
    /// <param name="associatedData">Optional additional authenticated data (must match encryption).</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="CryptographicException">
    /// Thrown when the key is invalid, data is corrupted, or authentication fails.
    /// </exception>
    public static byte[] Decrypt(
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> tag,
        ReadOnlySpan<byte> associatedData = default)
    {
        if (key.Length != CryptoConstants.AesKeySizeBytes)
            throw new ArgumentException($"Key must be {CryptoConstants.AesKeySizeBytes} bytes for AES-256", nameof(key));

        if (nonce.Length != CryptoConstants.AesNonceSizeBytes)
            throw new ArgumentException($"Nonce must be {CryptoConstants.AesNonceSizeBytes} bytes", nameof(nonce));

        if (tag.Length != CryptoConstants.AesTagSizeBytes)
            throw new ArgumentException($"Tag must be {CryptoConstants.AesTagSizeBytes} bytes", nameof(tag));

        var plaintext = new byte[ciphertext.Length];

        using var aesGcm = new System.Security.Cryptography.AesGcm(key, CryptoConstants.AesTagSizeBytes);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);

        return plaintext;
    }

    /// <summary>
    /// Generates a cryptographically secure random key.
    /// </summary>
    /// <returns>A random key of size AesKeySizeBytes (32 bytes for AES-256).</returns>
    public static byte[] GenerateKey()
    {
        var key = new byte[CryptoConstants.AesKeySizeBytes];
        RandomNumberGenerator.Fill(key);
        return key;
    }
}
