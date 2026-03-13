namespace Erebus.Core;

/// <summary>
/// Represents passport or ID document data
/// </summary>
public sealed class SecurePassportRecord : VaultRecord
{
    public SecurePassportRecord(
        RecordId id,
        string title,
        string fullName,
        string documentNumber,
        DateTime dateOfBirth,
        DateTime issueDate,
        DateTime expiryDate,
        string issuingAuthority,
        string? subdivisionCode = null,
        string country = "RUS",
        DateTime createdAt = default)
        : base(id, RecordType.SecPassport, title, createdAt == default ? DateTime.UtcNow : createdAt)
    {
        FullName = fullName;
        DocumentNumber = documentNumber;
        DateOfBirth = dateOfBirth;
        IssueDate = issueDate;
        ExpiryDate = expiryDate;
        IssuingAuthority = issuingAuthority;
        SubdivisionCode = subdivisionCode;
        Country = country;
    }

    public string FullName { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; }
    public string? SubdivisionCode { get; set; }

    /// <summary>
    /// Country code (ISO 3166-1 alpha-3)
    /// </summary>
    public string Country { get; set; }
}
