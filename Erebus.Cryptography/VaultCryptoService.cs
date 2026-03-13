namespace Erebus.Cryptography;

/// <summary>
/// High-level cryptographic service for vault operations.
/// Combines Argon2id, HKDF, and AES-GCM for secure vault management.
/// </summary>
public sealed class VaultCryptoService : IDisposable
{
    private byte[]? _masterKey;
    private bool _disposed;

    /// <summary>
    /// Derives the master encryption key from a password and salt using Argon2id.
    /// </summary>
    /// <param name="password">The master password.</param>
    /// <param name="salt">The salt stored with the vault.</param>
    public void InitializeFromPassword(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _masterKey = Argon2Hasher.Hash(password, salt);
    }

    /// <summary>
    /// Derives a file-specific encryption key from the master key using HKDF.
    /// </summary>
    /// <param name="fileId">Unique identifier for the file.</param>
    /// <returns>The derived file encryption key.</returns>
    public byte[] DeriveFileKey(string fileId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_masterKey == null) throw new InvalidOperationException("Master key not initialized");

        var info = System.Text.Encoding.UTF8.GetBytes($"file:{fileId}");
        return HkdfDeriver.DeriveKey(_masterKey, Array.Empty<byte>(), info);
    }

    /// <summary>
    /// Encrypts vault data using AES-256-GCM.
    /// </summary>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="associatedData">Optional additional authenticated data.</param>
    /// <returns>Encrypted data with nonce and tag.</returns>
    public (byte[] Nonce, byte[] Ciphertext, byte[] Tag) Encrypt(
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> associatedData = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_masterKey == null) throw new InvalidOperationException("Master key not initialized");

        return AesGcmCipher.Encrypt(_masterKey, plaintext, associatedData);
    }

    /// <summary>
    /// Decrypts vault data using AES-256-GCM.
    /// </summary>
    /// <param name="nonce">The nonce from encryption.</param>
    /// <param name="ciphertext">The encrypted data.</param>
    /// <param name="tag">The authentication tag.</param>
    /// <param name="associatedData">Optional additional authenticated data.</param>
    /// <returns>Decrypted plaintext.</returns>
    public byte[] Decrypt(
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> tag,
        ReadOnlySpan<byte> associatedData = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_masterKey == null) throw new InvalidOperationException("Master key not initialized");

        return AesGcmCipher.Decrypt(_masterKey, nonce, ciphertext, tag, associatedData);
    }

    /// <summary>
    /// Decrypts file content using the provided encryption key.
    /// Expects format: nonce (12 bytes) + tag (16 bytes) + ciphertext
    /// </summary>
    /// <param name="encryptedData">The encrypted file data with nonce and tag prepended.</param>
    /// <param name="encryptionKey">The encryption key for decryption.</param>
    /// <returns>Decrypted file content.</returns>
    public byte[] DecryptFile(byte[] encryptedData, byte[] encryptionKey)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        
        // Format: nonce (12) + tag (16) + ciphertext
        var nonceSize = CryptoConstants.AesNonceSizeBytes;
        var tagSize = CryptoConstants.AesTagSizeBytes;
        
        if (encryptedData.Length < nonceSize + tagSize)
            throw new ArgumentException("Encrypted data too short", nameof(encryptedData));
        
        var nonce = encryptedData.AsSpan(0, nonceSize);
        var tag = encryptedData.AsSpan(nonceSize, tagSize);
        var ciphertext = encryptedData.AsSpan(nonceSize + tagSize);
        
        return AesGcmCipher.Decrypt(encryptionKey, nonce, ciphertext, tag);
    }

    /// <summary>
    /// Creates a verification hash for password validation.
    /// This is stored in the 'verify' file and used to check password correctness.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="verifySalt">A dedicated salt for verification.</param>
    /// <returns>The verification hash.</returns>
    public static byte[] CreateVerificationHash(ReadOnlySpan<byte> password, ReadOnlySpan<byte> verifySalt)
    {
        return Argon2Hasher.Hash(password, verifySalt);
    }

    /// <summary>
    /// Verifies if a password matches the stored verification hash.
    /// </summary>
    /// <param name="password">The password to verify.</param>
    /// <param name="verifySalt">The stored verification salt.</param>
    /// <param name="storedHash">The stored verification hash.</param>
    /// <returns>True if password is correct, false otherwise.</returns>
    public static bool VerifyPassword(
        ReadOnlySpan<byte> password,
        ReadOnlySpan<byte> verifySalt,
        ReadOnlySpan<byte> storedHash)
    {
        var computedHash = CreateVerificationHash(password, verifySalt);
        try
        {
            return SecureMemoryHelper.ConstantTimeEquals(computedHash, storedHash);
        }
        finally
        {
            SecureMemoryHelper.SecureClear(computedHash);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (_masterKey != null)
            SecureMemoryHelper.SecureClear(_masterKey);
        _masterKey = null;
        _disposed = true;
    }
}
