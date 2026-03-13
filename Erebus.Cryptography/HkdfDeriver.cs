using System.Security.Cryptography;

namespace Erebus.Cryptography;

/// <summary>
/// Provides HKDF (HMAC-based Key Derivation Function) for deriving keys.
/// </summary>
public static class HkdfDeriver
{
    /// <summary>
    /// Derives a key using HKDF-SHA256.
    /// </summary>
    /// <param name="inputKeyMaterial">The input key material (e.g., master key).</param>
    /// <param name="salt">Optional salt (can be empty for single-input derivation).</param>
    /// <param name="info">Context/application-specific info (e.g., "file-encryption-key").</param>
    /// <param name="outputLength">Desired output length in bytes (default: 32).</param>
    /// <returns>The derived key.</returns>
    public static byte[] DeriveKey(
        ReadOnlySpan<byte> inputKeyMaterial,
        ReadOnlySpan<byte> salt,
        ReadOnlySpan<byte> info,
        int outputLength = CryptoConstants.HkdfOutputSizeBytes)
    {
        using var hkdf = new HmacSha256Kdf(inputKeyMaterial);
        return hkdf.DeriveBytes(salt, info, outputLength);
    }

    /// <summary>
    /// Derives multiple keys from a single master key.
    /// </summary>
    /// <param name="masterKey">The master key.</param>
    /// <param name="salt">Optional salt.</param>
    /// <param name="keyCount">Number of keys to derive.</param>
    /// <param name="outputLength">Length of each derived key.</param>
    /// <returns>Array of derived keys.</returns>
    public static byte[][] DeriveMultipleKeys(
        ReadOnlySpan<byte> masterKey,
        ReadOnlySpan<byte> salt,
        int keyCount,
        int outputLength = CryptoConstants.HkdfOutputSizeBytes)
    {
        var keys = new byte[keyCount][];
        for (int i = 0; i < keyCount; i++)
        {
            var info = System.Text.Encoding.UTF8.GetBytes($"key-{i}");
            keys[i] = DeriveKey(masterKey, salt, info, outputLength);
        }
        return keys;
    }
}

/// <summary>
/// Internal wrapper for HKDF using HMAC-SHA256.
/// </summary>
internal sealed class HmacSha256Kdf : IDisposable
{
    private readonly byte[] _key;
    private bool _disposed;

    public HmacSha256Kdf(ReadOnlySpan<byte> key)
    {
        _key = key.ToArray();
    }

    public byte[] DeriveBytes(ReadOnlySpan<byte> salt, ReadOnlySpan<byte> info, int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // HKDF-Extract
        using var hmac = new HMACSHA256(_key);
        var prk = salt.IsEmpty
            ? hmac.ComputeHash(Array.Empty<byte>())
            : hmac.ComputeHash(salt.ToArray());

        // HKDF-Expand
        var output = new byte[length];
        var previousBlock = Array.Empty<byte>();
        var counter = 1;
        var outputOffset = 0;

        while (outputOffset < length)
        {
            using var expandHmac = new HMACSHA256(prk);
            var block = new byte[previousBlock.Length + info.Length + 1];
            previousBlock.CopyTo(block, 0);
            info.CopyTo(block.AsSpan(previousBlock.Length));
            block[^1] = (byte)counter;

            var hash = expandHmac.ComputeHash(block);
            var copyLength = Math.Min(hash.Length, length - outputOffset);
            Array.Copy(hash, 0, output, outputOffset, copyLength);
            outputOffset += copyLength;
            previousBlock = hash;
            counter++;
        }

        // Secure cleanup
        Array.Clear(prk);
        return output;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Array.Clear(_key);
        _disposed = true;
    }
}
