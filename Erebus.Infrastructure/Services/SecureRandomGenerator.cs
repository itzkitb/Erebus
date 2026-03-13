using System.Security.Cryptography;
using Erebus.Core.Interfaces;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Service for generating secure random values
/// </summary>
public sealed class SecureRandomGenerator : ISecureRandomGenerator
{
    private static readonly string[] CommonWords = new[]
    {
        "correct", "horse", "battery", "staple", "purple", "monkey", "dishwasher",
        "nuclear", "waffle", "dragon", "knight", "castle", "forest", "mountain",
        "ocean", "river", "sunset", "sunrise", "thunder", "lightning", "shadow",
        "crystal", "diamond", "emerald", "ruby", "sapphire", "golden", "silver",
        "bronze", "copper", "iron", "steel", "titanium", "platinum", "quantum",
        "cosmic", "galaxy", "nebula", "comet", "asteroid", "planet", "starlight"
    }; // TODO: Add more words

    public string GeneratePassword(int length = 16, bool useUppercase = true, bool useLowercase = true, bool useDigits = true, bool useSymbols = true)
    {
        var charSet = "";
        if (useUppercase) charSet += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (useLowercase) charSet += "abcdefghijklmnopqrstuvwxyz";
        if (useDigits) charSet += "0123456789";
        if (useSymbols) charSet += "!@#$%^&*()_+-=[]{}|;:,.<>?";

        if (charSet.Length == 0)
            throw new ArgumentException("At least one character set must be selected");

        var password = new char[length];
        var randomData = new byte[length];
        RandomNumberGenerator.Fill(randomData);

        for (int i = 0; i < length; i++)
        {
            password[i] = charSet[randomData[i] % charSet.Length];
        }

        return new string(password);
    }

    public byte[] GenerateRandomBytes(int count)
    {
        var bytes = new byte[count];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    public string GeneratePassphrase(int wordCount = 4, string separator = "-")
    {
        var words = new string[wordCount];
        var randomData = new byte[wordCount * 2];
        RandomNumberGenerator.Fill(randomData);

        for (int i = 0; i < wordCount; i++)
        {
            var index = BitConverter.ToUInt16(randomData, i * 2) % CommonWords.Length;
            words[i] = CommonWords[index];
        }

        return string.Join(separator, words);
    }

    public string GenerateUsername(string? prefix = null)
    {
        var randomData = new byte[4];
        RandomNumberGenerator.Fill(randomData);
        var randomSuffix = BitConverter.ToUInt32(randomData, 0) % 10000;

        if (!string.IsNullOrEmpty(prefix))
        {
            return $"{prefix}{randomSuffix:D4}";
        }

        var adjectives = new[] { "happy", "quick", "silent", "bright", "calm", "wild", "swift", "clever" };
        var nouns = new[] { "panda", "eagle", "tiger", "wolf", "falcon", "cobra", "hawk", "lynx" };

        var adjIndex = RandomNumberGenerator.GetInt32(0, adjectives.Length);
        var nounIndex = RandomNumberGenerator.GetInt32(0, nouns.Length);

        return $"{adjectives[adjIndex]}{nouns[nounIndex]}{randomSuffix:D4}";
    }

    public string GenerateTotpSecret()
    {
        // 20 bytes for TOTP
        var secretBytes = new byte[20];
        RandomNumberGenerator.Fill(secretBytes);

        // base32
        return Base32Encode(secretBytes);
    }

    public string GenerateTotpCode(string secret)
    {
        // Decode base32
        var secretBytes = Base32Decode(secret);

        // Get current time step (30 secs)
        var timeStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var timeBytes = BitConverter.GetBytes(timeStep);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        // HMAC-SHA1
        using var hmac = new HMACSHA1(secretBytes);
        var hash = hmac.ComputeHash(timeBytes);

        // Dynamic truncation
        var offset = hash[hash.Length - 1] & 0x0F;
        var code = BitConverter.ToInt32(hash, offset) & 0x7FFFFFFF;
        var totpCode = code % 1000000;

        return totpCode.ToString("D6");
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new System.Text.StringBuilder();

        int buffer = 0;
        int bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                result.Append(alphabet[(buffer >> (bitsLeft - 5)) & 0x1F]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
        }

        return result.ToString();
    }

    private static byte[] Base32Decode(string data)
    {
        var upperData = data.ToUpperInvariant().TrimEnd('=');
        var result = new List<byte>();

        int buffer = 0;
        int bitsLeft = 0;

        foreach (var c in upperData)
        {
            int value = c switch
            {
                >= 'A' and <= 'Z' => c - 'A',
                >= '2' and <= '7' => c - '2' + 26,
                _ => throw new ArgumentException("Invalid base32 character")
            };

            buffer = (buffer << 5) | value;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                result.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        return result.ToArray();
    }
}
