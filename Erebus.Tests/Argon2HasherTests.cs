using Xunit;

namespace Erebus.Cryptography.Tests;

public class Argon2HasherTests
{
    [Fact]
    public void Hash_SamePasswordAndSalt_ReturnsSameHash()
    {
        // Arrange
        var password = System.Text.Encoding.UTF8.GetBytes("TestPassword123!");
        var salt = new byte[CryptoConstants.Argon2SaltSizeBytes];
        Array.Fill(salt, (byte)0x42);

        // Act
        var hash1 = Argon2Hasher.Hash(password, salt);
        var hash2 = Argon2Hasher.Hash(password, salt);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Hash_DifferentSalt_ReturnsDifferentHash()
    {
        // Arrange
        var password = System.Text.Encoding.UTF8.GetBytes("TestPassword123!");
        var salt1 = new byte[CryptoConstants.Argon2SaltSizeBytes];
        var salt2 = new byte[CryptoConstants.Argon2SaltSizeBytes];
        Array.Fill(salt1, (byte)0x42);
        Array.Fill(salt2, (byte)0x43);

        // Act
        var hash1 = Argon2Hasher.Hash(password, salt1);
        var hash2 = Argon2Hasher.Hash(password, salt2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_DifferentPassword_ReturnsDifferentHash()
    {
        // Arrange
        var password1 = System.Text.Encoding.UTF8.GetBytes("Password1");
        var password2 = System.Text.Encoding.UTF8.GetBytes("Password2");
        var salt = new byte[CryptoConstants.Argon2SaltSizeBytes];
        Array.Fill(salt, (byte)0x42);

        // Act
        var hash1 = Argon2Hasher.Hash(password1, salt);
        var hash2 = Argon2Hasher.Hash(password2, salt);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Hash_ReturnsCorrectLength()
    {
        // Arrange
        var password = System.Text.Encoding.UTF8.GetBytes("TestPassword");
        var salt = Argon2Hasher.GenerateSalt();
        var outputLength = 64;

        // Act
        var hash = Argon2Hasher.Hash(password, salt, outputLength);

        // Assert
        Assert.Equal(outputLength, hash.Length);
    }

    [Fact]
    public void GenerateSalt_ReturnsCorrectLength()
    {
        // Act
        var salt = Argon2Hasher.GenerateSalt();

        // Assert
        Assert.Equal(CryptoConstants.Argon2SaltSizeBytes, salt.Length);
    }

    [Fact]
    public void GenerateSalt_DifferentCalls_ReturnsDifferentSalts()
    {
        // Act
        var salt1 = Argon2Hasher.GenerateSalt();
        var salt2 = Argon2Hasher.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }
}
