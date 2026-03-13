using Xunit;

namespace Erebus.Cryptography.Tests;

public class VaultCryptoServiceTests : IDisposable
{
    private readonly byte[] _testPassword;
    private readonly byte[] _testSalt;

    public VaultCryptoServiceTests()
    {
        _testPassword = System.Text.Encoding.UTF8.GetBytes("MySecurePassword123!");
        _testSalt = Argon2Hasher.GenerateSalt();
    }

    [Fact]
    public void InitializeFromPassword_DerivesKey()
    {
        // Arrange
        using var service = new VaultCryptoService();

        // Act
        service.InitializeFromPassword(_testPassword, _testSalt);

        // Assert - should not throw, key is initialized internally
        // We verify by testing encrypt/decrypt functionality
    }

    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalPlaintext()
    {
        // Arrange
        using var service = new VaultCryptoService();
        service.InitializeFromPassword(_testPassword, _testSalt);
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret vault data");
        var associatedData = System.Text.Encoding.UTF8.GetBytes("vault-metadata");

        // Act
        var (nonce, ciphertext, tag) = service.Encrypt(plaintext, associatedData);
        var decrypted = service.Decrypt(nonce, ciphertext, tag, associatedData);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void DeriveFileKey_SameFileId_ReturnsSameKey()
    {
        // Arrange
        using var service = new VaultCryptoService();
        service.InitializeFromPassword(_testPassword, _testSalt);
        var fileId = "file-123";

        // Act
        var key1 = service.DeriveFileKey(fileId);
        var key2 = service.DeriveFileKey(fileId);

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void DeriveFileKey_DifferentFileId_ReturnsDifferentKey()
    {
        // Arrange
        using var service = new VaultCryptoService();
        service.InitializeFromPassword(_testPassword, _testSalt);
        var fileId1 = "file-123";
        var fileId2 = "file-456";

        // Act
        var key1 = service.DeriveFileKey(fileId1);
        var key2 = service.DeriveFileKey(fileId2);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveFileKey_ReturnsCorrectKeyLength()
    {
        // Arrange
        using var service = new VaultCryptoService();
        service.InitializeFromPassword(_testPassword, _testSalt);
        var fileId = "file-123";

        // Act
        var key = service.DeriveFileKey(fileId);

        // Assert
        Assert.Equal(CryptoConstants.HkdfOutputSizeBytes, key.Length);
    }

    [Fact]
    public void CreateVerificationHash_SamePassword_ReturnsSameHash()
    {
        // Arrange
        var password = System.Text.Encoding.UTF8.GetBytes("TestPassword");
        var salt = Argon2Hasher.GenerateSalt();

        // Act
        var hash1 = VaultCryptoService.CreateVerificationHash(password, salt);
        var hash2 = VaultCryptoService.CreateVerificationHash(password, salt);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = System.Text.Encoding.UTF8.GetBytes("CorrectPassword");
        var salt = Argon2Hasher.GenerateSalt();
        var storedHash = VaultCryptoService.CreateVerificationHash(password, salt);

        // Act
        var result = VaultCryptoService.VerifyPassword(password, salt, storedHash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var correctPassword = System.Text.Encoding.UTF8.GetBytes("CorrectPassword");
        var wrongPassword = System.Text.Encoding.UTF8.GetBytes("WrongPassword");
        var salt = Argon2Hasher.GenerateSalt();
        var storedHash = VaultCryptoService.CreateVerificationHash(correctPassword, salt);

        // Act
        var result = VaultCryptoService.VerifyPassword(wrongPassword, salt, storedHash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Dispose_ClearsMasterKey()
    {
        // Arrange
        var service = new VaultCryptoService();
        service.InitializeFromPassword(_testPassword, _testSalt);

        // Act
        service.Dispose();

        // Assert - service should be disposed, further operations should throw
        Assert.Throws<ObjectDisposedException>(() =>
            service.Encrypt(System.Text.Encoding.UTF8.GetBytes("test")));
    }

    [Fact]
    public void Encrypt_NotInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        using var service = new VaultCryptoService();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.Encrypt(System.Text.Encoding.UTF8.GetBytes("test")));
    }

    [Fact]
    public void Decrypt_NotInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        using var service = new VaultCryptoService();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.Decrypt(new byte[12], new byte[10], new byte[16]));
    }

    [Fact]
    public void DeriveFileKey_NotInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        using var service = new VaultCryptoService();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            service.DeriveFileKey("file-123"));
    }

    public void Dispose()
    {
        SecureMemoryHelper.SecureClear(_testPassword);
        SecureMemoryHelper.SecureClear(_testSalt);
    }
}
