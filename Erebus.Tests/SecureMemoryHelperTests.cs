using System.Security;
using Xunit;

namespace Erebus.Cryptography.Tests;

public class SecureMemoryHelperTests
{
    [Fact]
    public void SecureStringToBytes_ConvertsCorrectly()
    {
        // Arrange
        var secureString = new SecureString();
        secureString.AppendChar('H');
        secureString.AppendChar('e');
        secureString.AppendChar('l');
        secureString.AppendChar('l');
        secureString.AppendChar('o');
        secureString.MakeReadOnly();

        // Act
        var bytes = SecureMemoryHelper.SecureStringToBytes(secureString);

        // Assert
        var expected = System.Text.Encoding.UTF8.GetBytes("Hello");
        Assert.Equal(expected, bytes);
    }

    [Fact]
    public void SecureStringToBytes_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var secureString = new SecureString();
        secureString.MakeReadOnly();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SecureMemoryHelper.SecureStringToBytes(secureString));
    }

    [Fact]
    public void SecureStringToBytes_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SecureMemoryHelper.SecureStringToBytes(null!));
    }

    [Fact]
    public void BytesToSecureString_ConvertsCorrectly()
    {
        // Arrange
        var bytes = System.Text.Encoding.UTF8.GetBytes("Hello");

        // Act
        var secureString = SecureMemoryHelper.BytesToSecureString(bytes);

        // Assert
        Assert.Equal(5, secureString.Length);
    }

    [Fact]
    public void SecureClear_ClearsByteArray()
    {
        // Arrange
        var array = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        SecureMemoryHelper.SecureClear(array);

        // Assert
        Assert.All(array, b => Assert.Equal(0, b));
    }

    [Fact]
    public void SecureClear_Null_DoesNotThrow()
    {
        // Act & Assert
        var exception = Record.Exception(() => SecureMemoryHelper.SecureClear((byte[])null!));
        Assert.Null(exception);
    }

    [Fact]
    public void SecureClear_MultipleArrays_ClearsAll()
    {
        // Arrange
        var array1 = new byte[] { 1, 2, 3 };
        var array2 = new byte[] { 4, 5, 6 };

        // Act
        SecureMemoryHelper.SecureClear(array1, array2);

        // Assert
        Assert.All(array1, b => Assert.Equal(0, b));
        Assert.All(array2, b => Assert.Equal(0, b));
    }

    [Fact]
    public void ConstantTimeEquals_EqualArrays_ReturnsTrue()
    {
        // Arrange
        var array1 = new byte[] { 1, 2, 3, 4, 5 };
        var array2 = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = SecureMemoryHelper.ConstantTimeEquals(array1, array2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ConstantTimeEquals_DifferentArrays_ReturnsFalse()
    {
        // Arrange
        var array1 = new byte[] { 1, 2, 3, 4, 5 };
        var array2 = new byte[] { 1, 2, 3, 4, 6 };

        // Act
        var result = SecureMemoryHelper.ConstantTimeEquals(array1, array2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConstantTimeEquals_DifferentLengths_ReturnsFalse()
    {
        // Arrange
        var array1 = new byte[] { 1, 2, 3, 4, 5 };
        var array2 = new byte[] { 1, 2, 3, 4 };

        // Act
        var result = SecureMemoryHelper.ConstantTimeEquals(array1, array2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ConstantTimeEquals_EmptyArrays_ReturnsTrue()
    {
        // Arrange
        var array1 = Array.Empty<byte>();
        var array2 = Array.Empty<byte>();

        // Act
        var result = SecureMemoryHelper.ConstantTimeEquals(array1, array2);

        // Assert
        Assert.True(result);
    }
}
