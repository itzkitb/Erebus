namespace Erebus.Core;

/// <summary>
/// Represents a secure note.
/// </summary>
public sealed class SecureNoteRecord : VaultRecord
{
    public SecureNoteRecord(RecordId id, string title, string content, string format = "md", DateTime createdAt = default)
        : base(id, RecordType.SecNote, title, createdAt == default ? DateTime.UtcNow : createdAt)
    {
        Content = content;
        Format = format;
    }

    public string Content { get; set; }

    /// <summary>
    /// Content format: "md" for Markdown, "html" for HTML.
    /// </summary>
    public string Format { get; set; } = "md";
}
