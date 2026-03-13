namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for file upload
/// </summary>
public sealed class UploadFileDto
{
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? Folder { get; set; }
    public List<string> Tags { get; set; } = new();
}
