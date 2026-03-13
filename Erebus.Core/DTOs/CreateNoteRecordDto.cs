namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for creating a note record
/// </summary>
public sealed class CreateNoteRecordDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Format { get; set; } = "md";
    public string? Folder { get; set; }
    public List<string> Tags { get; set; } = new();
}
