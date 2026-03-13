namespace Erebus.Core;

/// <summary>
/// Base class for all vault records.
/// </summary>
public abstract class VaultRecord
{
    protected VaultRecord(RecordId id, RecordType type, string title, DateTime createdAt)
    {
        Id = id;
        Type = type;
        Title = title;
        CreatedAt = createdAt;
        ModifiedAt = createdAt;
    }

    public RecordId Id { get; }
    public RecordType Type { get; }
    public string Title { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Folder { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime ModifiedAt { get; protected set; }

    /// <summary>
    /// Updates the timestamp
    /// </summary>
    protected void Touch() => ModifiedAt = DateTime.UtcNow;
}
