using Erebus.Core;

namespace Erebus.Core.Services;

/// <summary>
/// Individual password health item
/// </summary>
public sealed class PasswordHealthItem
{
    public RecordId RecordId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int StrengthScore { get; set; }
    public bool IsBreached { get; set; }
    public int BreachCount { get; set; }
    public bool IsReused { get; set; }
}
