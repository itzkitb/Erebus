namespace Erebus.Core;

/// <summary>
/// Represents an encrypted file stored in the vault
/// </summary>
public sealed class SecureFileRecord : VaultRecord
{
    public SecureFileRecord(RecordId id, string title, string fileName, long fileSize, string mimeType, DateTime createdAt)
        : base(id, RecordType.SecFile, title, createdAt)
    {
        FileName = fileName;
        FileSize = fileSize;
        MimeType = mimeType;
    }

    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string MimeType { get; set; }
    public string EncryptedFilePath { get; set; } = string.Empty;
}
