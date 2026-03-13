using System.Security.Cryptography;
using Konscious.Security.Cryptography;

namespace Erebus.Cryptography;

/// <summary>
/// Provides Argon2id password hashing functionality.
/// </summary>
public static class Argon2Hasher
{
    /// <summary>
    /// Hashes a password using Argon2id algorithm.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <param name="salt">The salt (should be Argon2SaltSizeBytes).</param>
    /// <param name="outputLength">The desired hash output length in bytes.</param>
    /// <returns>The hash as a byte array.</returns>
    public static byte[] Hash(ReadOnlySpan<byte> password, ReadOnlySpan<byte> salt, int outputLength = CryptoConstants.AesKeySizeBytes)
    {
        using var argon2 = new Argon2id(password.ToArray())
        {
            Salt = salt.ToArray(),
            MemorySize = CryptoConstants.Argon2MemoryCostKib,
            Iterations = CryptoConstants.Argon2TimeCost,
            DegreeOfParallelism = CryptoConstants.Argon2Parallelism
        };

        return argon2.GetBytes(outputLength);
    }

    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <returns>A random salt of size Argon2SaltSizeBytes.</returns>
    public static byte[] GenerateSalt()
    {
        var salt = new byte[CryptoConstants.Argon2SaltSizeBytes];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}
