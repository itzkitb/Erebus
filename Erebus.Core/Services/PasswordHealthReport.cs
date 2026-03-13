using Erebus.Core;

namespace Erebus.Core.Services;

/// <summary>
/// Report on password health in a vault
/// </summary>
public sealed class PasswordHealthReport
{
    public int TotalPasswords { get; set; }
    public int WeakPasswords { get; set; }
    public int BreachedPasswords { get; set; }
    public int ReusedPasswords { get; set; }
    public List<PasswordHealthItem> Items { get; set; } = new();
}
