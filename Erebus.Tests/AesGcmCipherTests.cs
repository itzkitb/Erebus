using System.Security.Cryptography;
using Xunit;

namespace Erebus.Cryptography.Tests;

public class AesGcmCipherTests
{
    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalPlaintext()
    {
        // Arrange
        var key = AesGcmCipher.GenerateKey();
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World! This is a secret message.");
        var associatedData = System.Text.Encoding.UTF8.GetBytes("additional-data");

        // Act
        var (nonce, ciphertext, tag) = AesGcmCipher.Encrypt(key, plaintext, associatedData);
        var decrypted = AesGcmCipher.Decrypt(key, nonce, ciphertext, tag, associatedData);

        // Assert
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_DifferentNonce_ProducesDifferentCiphertext()
    {
        // Arrange
        var key = AesGcmCipher.GenerateKey();
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, World!");

        // Act
        var (nonce1, ciphertext1, tag1) = AesGcmCipher.Encrypt(key, plaintext);
        var (nonce2, ciphertext2, tag2) = AesGcmCipher.Encrypt(key, plaintext);

        // Assert
        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void Decrypt_WrongKey_ThrowsCryptographicException()
    {
        // Arrange
        var key1 = AesGcmCipher.GenerateKey();
        var key2 = AesGcmCipher.GenerateKey();
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret message");
        var (nonce, ciphertext, tag) = AesGcmCipher.Encrypt(key1, plaintext);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            AesGcmCipher.Decrypt(key2, nonce, ciphertext, tag));
    }

    [Fact]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        var key = AesGcmCipher.GenerateKey();
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret message");
        var (nonce, ciphertext, tag) = AesGcmCipher.Encrypt(key, plaintext);

        // Tamper with ciphertext
        ciphertext[0] ^= 0xFF;

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            AesGcmCipher.Decrypt(key, nonce, ciphertext, tag));
    }

    [Fact]
    public void Decrypt_WrongAssociatedData_ThrowsCryptographicException()
    {
        // Arrange
        var key = AesGcmCipher.GenerateKey();
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Secret message");
        var associatedData1 = System.Text.Encoding.UTF8.GetBytes("data1");
        var associatedData2 = System.Text.Encoding.UTF8.GetBytes("data2");
        var (nonce, ciphertext, tag) = AesGcmCipher.Encrypt(key, plaintext, associatedData1);

        // Act & Assert
        Assert.ThrowsAny<CryptographicException>(() =>
            AesGcmCipher.Decrypt(key, nonce, ciphertext, tag, associatedData2));
    }

    [Fact]
    public void GenerateKey_ReturnsCorrectLength()
    {
        // Act
        var key = AesGcmCipher.GenerateKey();

        // Assert
        Assert.Equal(CryptoConstants.AesKeySizeBytes, key.Length);
    }

    [Fact]
    public void GenerateKey_DifferentCalls_ReturnsDifferentKeys()
    {
        // Act
        var key1 = AesGcmCipher.GenerateKey();
        var key2 = AesGcmCipher.GenerateKey();

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void Encrypt_EmptyPlaintext_ReturnsEmptyCiphertext()
    {
        // Arrange
        var key = AesGcmCipher.GenerateKey();
        var plaintext = Array.Empty<byte>();

        // Act
        var (nonce, ciphertext, tag) = AesGcmCipher.Encrypt(key, plaintext);

        // Assert
        Assert.Empty(ciphertext);
        Assert.Equal(CryptoConstants.AesNonceSizeBytes, nonce.Length);
        Assert.Equal(CryptoConstants.AesTagSizeBytes, tag.Length);
    }
}
