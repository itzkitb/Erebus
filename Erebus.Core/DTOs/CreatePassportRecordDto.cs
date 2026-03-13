namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for creating a passport record
/// </summary>
public sealed class CreatePassportRecordDto
{
    public string Title { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string IssuingAuthority { get; set; } = string.Empty;
    public string? SubdivisionCode { get; set; }
    public string Country { get; set; } = "RUS";
    public string? Folder { get; set; }
    public List<string> Tags { get; set; } = new();
}
