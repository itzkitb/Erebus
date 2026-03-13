using Erebus.Core;
using Erebus.Core.Interfaces;
using Erebus.Core.Services;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Service for analyzing password health and strength
/// </summary>
public sealed class PasswordHealthService : IPasswordHealthService
{
    /// <summary>
    /// Analyzes the strength of a password
    /// </summary>
    public PasswordStrengthResult AnalyzePassword(string password)
    {
        var result = new PasswordStrengthResult
        {
            Length = password?.Length ?? 0,
            HasUppercase = password?.Any(char.IsUpper) ?? false,
            HasLowercase = password?.Any(char.IsLower) ?? false,
            HasDigits = password?.Any(char.IsDigit) ?? false,
            HasSymbols = password?.Any(c => !char.IsLetterOrDigit(c)) ?? false
        };

        if (string.IsNullOrEmpty(password))
        {
            result.Score = 0;
            result.Feedback = "Пароль не может быть пустым";
            return result;
        }

        int score = 0;
        var feedback = new List<string>();

        // Length scoring
        if (password.Length >= 8) score++;
        else feedback.Add("Используйте не менее 8 символов");
        
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        // Character variety
        var varietyCount = new[] { result.HasLowercase, result.HasUppercase, result.HasDigits, result.HasSymbols }.Count(b => b);
        
        if (varietyCount >= 3) score++;
        else if (varietyCount < 2) feedback.Add("Используйте буквы в разных регистрах, цифры и символы");

        if (varietyCount >= 4) score++;

        // Normalize to 0-4 scale
        result.Score = Math.Clamp(score, 0, 4);
        result.Feedback = feedback.Count > 0 ? string.Join(". ", feedback) : "Надежный пароль";

        return result;
    }

    /// <summary>
    /// Analyzes the strength of a password (legacy)
    /// </summary>
    public int AnalyzePasswordStrength(string password)
    {
        return AnalyzePassword(password).Score;
    }

    /// <summary>
    /// Checks if a password has been pwned using Have I Been Pwned API.
    /// Returns number of times the password has been seen in breaches 
    /// (That means that u shold'nt use this pass)
    /// </summary>
    public async Task<int> CheckPasswordBreachAsync(string password, CancellationToken cancellationToken = default)
    {
        // Hash the password using SHA1
        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hashBytes = sha1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

        // Get first 5 characters for k-anonymity
        var prefix = hash.Substring(0, 5);
        var suffix = hash.Substring(5);

        try
        {
            // Call HIBP API
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Erebus-Password-Manager/1.0");

            var response = await httpClient.GetAsync(
                $"https://api.pwnedpasswords.com/range/{prefix}",
                cancellationToken
            );

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var lines = content.Split('\n');

                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 2 && parts[0].Equals(suffix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(parts[1], out var count))
                            return count;
                    }
                }
            }
        }
        catch
        {
            // Something went wrong
            // GG WP
        }

        return 0;
    }

    /// <summary>
    /// Checks if a password has been compromised
    /// </summary>
    public async Task<bool> IsPasswordCompromisedAsync(string password, CancellationToken cancellationToken = default)
    {
        var breachCount = await CheckPasswordBreachAsync(password, cancellationToken);
        return breachCount > 0;
    }

    /// <summary>
    /// Gets password health report for all passwords in a vault
    /// </summary>
    public async Task<PasswordHealthReport> GetVaultPasswordHealthAsync(IVaultRecordRepository vault, CancellationToken cancellationToken = default)
    {
        var report = new PasswordHealthReport();
        var passwordHashes = new Dictionary<string, List<RecordId>>();

        var records = await vault.GetAllRecordsAsync(cancellationToken);
        var passwordRecords = records.OfType<SecurePasswordRecord>().ToList();

        report.TotalPasswords = passwordRecords.Count;

        foreach (var record in passwordRecords)
        {
            var item = new PasswordHealthItem
            {
                RecordId = record.Id,
                Title = record.Title,
                StrengthScore = AnalyzePasswordStrength(record.Password)
            };

            // Track for reuse detection
            var passwordHash = GetPasswordHash(record.Password);
            if (!passwordHashes.ContainsKey(passwordHash))
                passwordHashes[passwordHash] = new List<RecordId>();
            passwordHashes[passwordHash].Add(record.Id);

            // Check breach status (passwords with low strength)
            if (item.StrengthScore <= 2)
            {
                var breachCount = await CheckPasswordBreachAsync(record.Password, cancellationToken);
                item.IsBreached = breachCount > 0;
                item.BreachCount = breachCount;

                if (item.IsBreached)
                    report.BreachedPasswords++;
            }

            if (item.StrengthScore <= 1)
                report.WeakPasswords++;

            report.Items.Add(item);
        }

        // Mark reused passwords
        foreach (var kvp in passwordHashes)
        {
            if (kvp.Value.Count > 1)
            {
                report.ReusedPasswords += kvp.Value.Count - 1;
                foreach (var recordId in kvp.Value)
                {
                    var item = report.Items.FirstOrDefault(i => i.RecordId == recordId);
                    if (item != null)
                        item.IsReused = true;
                }
            }
        }

        return report;
    }

    private static string GetPasswordHash(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashBytes);
    }
}
