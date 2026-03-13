using Erebus.Core.Interfaces;

namespace Erebus.Desktop.Services;

/// <summary>
/// Photino implementation of clipboard service.
/// Uses platform-specific CLI tools for clipboard access.
/// </summary>
public sealed class PhotinoClipboardService : IClipboardService
{
    public Task CopyAsync(string text, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c echo {EscapeShell(text)} | clip",
                        UseShellExecute = false,
                        RedirectStandardInput = true
                    };
                    using var proc = System.Diagnostics.Process.Start(psi);
                    proc?.WaitForExit();
                }
                else if (OperatingSystem.IsLinux())
                {
                    // Try xclip first
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "xclip",
                        Arguments = "-selection clipboard",
                        RedirectStandardInput = true,
                        UseShellExecute = false
                    };
                    using var proc = System.Diagnostics.Process.Start(psi);
                    if (proc != null)
                    {
                        proc.StandardInput.Write(text);
                        proc.WaitForExit();
                    }
                    else
                    {
                        // Fallback to wl-clipboard
                        psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "wl-copy",
                            Arguments = text,
                            UseShellExecute = false
                        };
                        System.Diagnostics.Process.Start(psi)?.WaitForExit();
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "pbcopy",
                        RedirectStandardInput = true,
                        UseShellExecute = false
                    };
                    using var proc = System.Diagnostics.Process.Start(psi);
                    proc?.StandardInput.Write(text);
                    proc?.WaitForExit();
                }
            }
            catch
            {
                // Clipboard operation failed silently
            }
        }, cancellationToken);
    }
    
    public Task<string?> GetTextAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
        // Read from clipboard is more complex and platform-specific
        // For MVP, we only implement write operations
    }
    
    private static string EscapeShell(string text)
    {
        return text.Replace("\"", "\"\"").Replace("&", "^&").Replace("|", "^|").Replace("<", "^<").Replace(">", "^>");
    }
}
