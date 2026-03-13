using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Erebus.Core;
using Erebus.Core.Interfaces;
using Erebus.Cryptography;
using Erebus.Infrastructure.Repositories;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Service for exporting and importing vault data in encrypted JSON format
/// </summary>
public sealed class ExportImportService : IExportImportService
{
    /// <summary>
    /// Exports vault data to encrypted JSON
    /// </summary>
    public async Task<byte[]> ExportVaultAsync(IVaultRecordRepository repository, VaultInfo vaultInfo, string exportPassword, CancellationToken cancellationToken = default)
    {
        var records = await repository.GetAllRecordsAsync(cancellationToken);
        
        // Create model
        var exportData = new VaultExport
        {
            Version = "1.0",
            ExportedAt = DateTime.UtcNow,
            VaultName = vaultInfo.Name,
            Records = records.Select(r => ConvertRecordToExportModel(r)).ToList()
        };

        // Serialize
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonData = JsonSerializer.Serialize(exportData, jsonOptions);
        var jsonBytes = Encoding.UTF8.GetBytes(jsonData);

        // Encrypt
        var salt = Argon2Hasher.GenerateSalt();
        var key = VaultRecordRepository.DeriveKey(exportPassword, salt);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var encryptedBytes = encryptor.TransformFinalBlock(jsonBytes, 0, jsonBytes.Length);
        
        // Combine
        var output = new byte[salt.Length + aes.IV.Length + encryptedBytes.Length];
        
        Buffer.BlockCopy(salt, 0, output, 0, salt.Length);
        Buffer.BlockCopy(aes.IV, 0, output, salt.Length, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, output, salt.Length + aes.IV.Length, encryptedBytes.Length);

        return output;
    }

    /// <summary>
    /// Imports vault data from encrypted JSON
    /// </summary>
    public async Task<VaultInfo> ImportVaultAsync(byte[] encryptedData, string importPassword, IVaultService vaultService, CancellationToken cancellationToken = default)
    {
        // Parse encrypted data
        var salt = new byte[16];
        var iv = new byte[16];
        
        Buffer.BlockCopy(encryptedData, 0, salt, 0, 16);
        Buffer.BlockCopy(encryptedData, 16, iv, 0, 16);
        
        var encryptedBytes = new byte[encryptedData.Length - 32];
        Buffer.BlockCopy(encryptedData, 32, encryptedBytes, 0, encryptedBytes.Length);

        // Derive key
        var key = VaultRecordRepository.DeriveKey(importPassword, salt);

        // Decrypt
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        var jsonData = Encoding.UTF8.GetString(decryptedBytes);

        // Parse JSON
        var exportData = JsonSerializer.Deserialize<VaultExport>(jsonData)
            ?? throw new InvalidOperationException("Invalid export data");

        // Create new vault
        var vault = await vaultService.CreateVaultAsync(
            exportData.VaultName,
            importPassword,
            $"Imported from backup on {DateTime.UtcNow:yyyy-MM-dd}",
            cancellationToken
        );

        return vault;
    }

    private static ExportRecordModel ConvertRecordToExportModel(VaultRecord record)
    {
        return record switch
        {
            SecurePasswordRecord pr => new ExportRecordModel
            {
                Type = "password",
                Title = pr.Title,
                Folder = pr.Folder,
                Tags = pr.Tags,
                Data = new Dictionary<string, object?>
                {
                    ["username"] = pr.Username,
                    ["password"] = pr.Password,
                    ["url"] = pr.Url,
                    ["displayName"] = pr.DisplayName
                }
            },
            SecureNoteRecord nr => new ExportRecordModel
            {
                Type = "note",
                Title = nr.Title,
                Folder = nr.Folder,
                Tags = nr.Tags,
                Data = new Dictionary<string, object?>
                {
                    ["content"] = nr.Content,
                    ["format"] = nr.Format
                }
            },
            SecurePassportRecord pp => new ExportRecordModel
            {
                Type = "passport",
                Title = pp.Title,
                Folder = pp.Folder,
                Tags = pp.Tags,
                Data = new Dictionary<string, object?>
                {
                    ["fullName"] = pp.FullName,
                    ["documentNumber"] = pp.DocumentNumber,
                    ["dateOfBirth"] = pp.DateOfBirth.ToString("yyyy-MM-dd"),
                    ["issueDate"] = pp.IssueDate.ToString("yyyy-MM-dd"),
                    ["expiryDate"] = pp.ExpiryDate.ToString("yyyy-MM-dd"),
                    ["issuingAuthority"] = pp.IssuingAuthority,
                    ["subdivisionCode"] = pp.SubdivisionCode,
                    ["country"] = pp.Country
                }
            },
            SecureFileRecord fr => new ExportRecordModel
            {
                Type = "file",
                Title = fr.Title,
                Folder = fr.Folder,
                Tags = fr.Tags,
                Data = new Dictionary<string, object?>
                {
                    ["fileName"] = fr.FileName,
                    ["fileSize"] = fr.FileSize,
                    ["mimeType"] = fr.MimeType
                }
            },
            _ => throw new InvalidOperationException($"Unknown record type: {record.Type}")
        };
    }
}

/// <summary>
/// Export data model for JSON serialization
/// </summary>
public sealed class VaultExport
{
    public string Version { get; set; } = "1.0";
    public DateTime ExportedAt { get; set; }
    public string VaultName { get; set; } = string.Empty;
    public List<ExportRecordModel> Records { get; set; } = new();
}

/// <summary>
/// Export record model for JSON serialization
/// </summary>
public sealed class ExportRecordModel
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Folder { get; set; }
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object?> Data { get; set; } = new();
}
