using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Erebus.Core.Interfaces;

namespace Erebus.Desktop.Services;

/// <summary>
/// Photino implementation of clipboard service.
/// Tries JSInterop first, then falls back to platform-specific methods.
/// </summary>
public sealed class PhotinoClipboardService : IClipboardService
{
    private readonly IServiceProvider _serviceProvider;

    public PhotinoClipboardService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task CopyAsync(string text, CancellationToken cancellationToken = default)
    {
        // Try JSInterop first (works in WebView context)
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jsRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            await jsRuntime.InvokeVoidAsync("erebusClipboard.copyText", cancellationToken, text);
            return;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // JSInterop failed, try native methods
            Console.Error.WriteLine($"JSInterop clipboard failed: {ex.Message}");
        }

        // Fallback to native Linux methods
        await CopyNativeAsync(text, cancellationToken);
    }

    private async Task CopyNativeAsync(string text, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            // Try xclip first
            if (TryClipboardWithTool("xclip", "-selection clipboard", text)) return;
            
            // Try xsel
            if (TryClipboardWithTool("xsel", "--clipboard --input", text)) return;
            
            // Try wl-copy (Wayland)
            if (TryClipboardWithTool("wl-copy", "", text)) return;

            // Try using DBus directly (works on most modern Linux desktops)
            if (TryClipboardViaDBus(text)) return;

            Console.Error.WriteLine("All clipboard methods failed. Install xclip, xsel, or wl-copy.");
        }, cancellationToken);
    }

    private static bool TryClipboardWithTool(string tool, string args, string text)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc != null)
            {
                proc.StandardInput.Write(text);
                proc.StandardInput.Close();
                proc.WaitForExit();
                if (proc.ExitCode == 0) return true;
            }
        }
        catch
        {
            // Tool not available or failed
        }
        return false;
    }

    private static bool TryClipboardViaDBus(string text)
    {
        try
        {
            // Use dbus-send to call clipboard service
            // This works on GNOME/KDE without additional tools
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dbus-send",
                Arguments = "--session --dest=org.freedesktop.portal.Desktop " +
                           "--type=method_call /org/freedesktop/portal/desktop " +
                           "org.freedesktop.portal.Clipboard.SetContents " +
                           $"string:\"{text.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit(1000);
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public Task<string?> GetTextAsync(CancellationToken cancellationToken = default)
    {
        // Read from clipboard requires user gesture and is not supported
        return Task.FromResult<string?>(null);
    }
}
