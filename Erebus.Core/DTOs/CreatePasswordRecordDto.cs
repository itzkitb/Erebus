namespace Erebus.Core.DTOs;

/// <summary>
/// Data transfer object for creating a password record
/// </summary>
public sealed class CreatePasswordRecordDto
{
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? OtpSecret { get; set; }
    public string? DisplayName { get; set; }
    public string? Folder { get; set; }
    public List<string> Tags { get; set; } = new();
}
