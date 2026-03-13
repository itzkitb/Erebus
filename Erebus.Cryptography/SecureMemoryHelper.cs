using System.Runtime.InteropServices;
using System.Security;

namespace Erebus.Cryptography;

/// <summary>
/// Helper methods for secure memory handling.
/// </summary>
public static class SecureMemoryHelper
{
    /// <summary>
    /// Converts a SecureString to a byte array (UTF-8 encoded).
    /// Caller is responsible for securely clearing the returned array.
    /// </summary>
    public static byte[] SecureStringToBytes(SecureString secureString)
    {
        if (secureString == null) throw new ArgumentNullException(nameof(secureString));
        if (secureString.Length == 0) throw new ArgumentException("SecureString cannot be empty", nameof(secureString));

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            var length = secureString.Length;
            var bytes = new byte[length * 2]; // UTF-16, 2 bytes per char
            Marshal.Copy(ptr, bytes, 0, bytes.Length);

            // Convert to UTF-8
            var utf8Bytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.UTF8, bytes);
            return utf8Bytes;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    /// <summary>
    /// Converts a byte array to a SecureString.
    /// </summary>
    public static SecureString BytesToSecureString(ReadOnlySpan<byte> bytes)
    {
        var unicodeBytes = System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.Unicode, bytes.ToArray());
        var ptr = Marshal.AllocHGlobal(unicodeBytes.Length);
        try
        {
            Marshal.Copy(unicodeBytes, 0, ptr, unicodeBytes.Length);
            var secureString = new SecureString();
            var charCount = unicodeBytes.Length / 2;
            for (int i = 0; i < charCount; i++)
            {
                var ch = (char)BitConverter.ToUInt16(unicodeBytes, i * 2);
                secureString.AppendChar(ch);
            }
            secureString.MakeReadOnly();
            return secureString;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    /// <summary>
    /// Securely clears a byte array by setting all bytes to zero.
    /// </summary>
    public static void SecureClear(byte[] array)
    {
        if (array == null) return;
        Array.Clear(array);
    }

    /// <summary>
    /// Securely clears multiple byte arrays.
    /// </summary>
    public static void SecureClear(params byte[][] arrays)
    {
        foreach (var array in arrays)
        {
            SecureClear(array);
        }
    }

    /// <summary>
    /// Compares two byte arrays in constant time to prevent timing attacks.
    /// </summary>
    /// <returns>True if arrays are equal, false otherwise.</returns>
    public static bool ConstantTimeEquals(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        if (a.Length != b.Length)
            return false;

        byte result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= (byte)(a[i] ^ b[i]);
        }
        return result == 0;
    }
}
