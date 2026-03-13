using Xunit;

namespace Erebus.Cryptography.Tests;

public class HkdfDeriverTests
{
    [Fact]
    public void DeriveKey_SameInput_ReturnsSameKey()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = new byte[16];
        Array.Fill(salt, (byte)0x43);
        var info = System.Text.Encoding.UTF8.GetBytes("test-info");

        // Act
        var key1 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info);
        var key2 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info);

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void DeriveKey_DifferentInfo_ReturnsDifferentKey()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = new byte[16];
        Array.Fill(salt, (byte)0x43);
        var info1 = System.Text.Encoding.UTF8.GetBytes("info1");
        var info2 = System.Text.Encoding.UTF8.GetBytes("info2");

        // Act
        var key1 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info1);
        var key2 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info2);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveKey_DifferentSalt_ReturnsDifferentKey()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt1 = new byte[16];
        var salt2 = new byte[16];
        Array.Fill(salt1, (byte)0x43);
        Array.Fill(salt2, (byte)0x44);
        var info = System.Text.Encoding.UTF8.GetBytes("test-info");

        // Act
        var key1 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt1, info);
        var key2 = HkdfDeriver.DeriveKey(inputKeyMaterial, salt2, info);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveKey_CustomOutputLength_ReturnsCorrectLength()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = new byte[16];
        var info = System.Text.Encoding.UTF8.GetBytes("test-info");
        var outputLength = 64;

        // Act
        var key = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info, outputLength);

        // Assert
        Assert.Equal(outputLength, key.Length);
    }

    [Fact]
    public void DeriveMultipleKeys_ReturnsCorrectCount()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = new byte[16];
        var keyCount = 5;

        // Act
        var keys = HkdfDeriver.DeriveMultipleKeys(inputKeyMaterial, salt, keyCount);

        // Assert
        Assert.Equal(keyCount, keys.Length);
    }

    [Fact]
    public void DeriveMultipleKeys_AllKeysAreUnique()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = new byte[16];
        var keyCount = 10;

        // Act
        var keys = HkdfDeriver.DeriveMultipleKeys(inputKeyMaterial, salt, keyCount);

        // Assert
        for (int i = 0; i < keyCount; i++)
        {
            for (int j = i + 1; j < keyCount; j++)
            {
                Assert.NotEqual(keys[i], keys[j]);
            }
        }
    }

    [Fact]
    public void DeriveKey_EmptySalt_WorksCorrectly()
    {
        // Arrange
        var inputKeyMaterial = new byte[32];
        Array.Fill(inputKeyMaterial, (byte)0x42);
        var salt = Array.Empty<byte>();
        var info = System.Text.Encoding.UTF8.GetBytes("test-info");

        // Act
        var key = HkdfDeriver.DeriveKey(inputKeyMaterial, salt, info);

        // Assert
        Assert.Equal(CryptoConstants.HkdfOutputSizeBytes, key.Length);
    }
}
