namespace Erebus.Core.DTOs;

/// <summary>
/// View model for displaying a vault record
/// </summary>
public sealed class RecordViewModel
{
    public RecordId Id { get; set; }
    public RecordType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Folder { get; set; }

    // Password-specific
    public string? Username { get; set; }
    public string? Url { get; set; }
    public bool HasOtp { get; set; }

    // File-specific
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? FileSizeDisplay => FileSize.HasValue ? FormatFileSize(FileSize.Value) : null;

    // Passport-specific
    public string? DocumentNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
