using Erebus.Core.Interfaces;

namespace Erebus.Infrastructure.Services;

/// <summary>
/// Biometric authentication service implementation
/// (Platform-specific)
/// </summary>
public sealed class BiometricService : IBiometricService
{
    private readonly bool _isAvailable;

    public BiometricService()
    {
        // Check platform availability
        _isAvailable = CheckPlatform();
    }

    private static bool CheckPlatform()
    {
#if WINDOWS
        // Windows Hello
        return true;
#elif ANDROID
        // Android Biometric
        return true;
#elif IOS
        // TouchID/FaceID
        return false;
#elif MACCATALYST
        // TouchID
        return false;
#else
        // Linux and other
        return false;
#endif
    }

    /// <summary>
    /// Checks if biometric authentication is available on this device
    /// </summary>
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isAvailable);
    }

    /// <summary>
    /// Authenticates the user using biometrics
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        return await AuthenticateAsync("Требуется подтверждение", cancellationToken);
    }

    /// <summary>
    /// Authenticates the user using biometrics with custom prompt
    /// </summary>
    public async Task<bool> AuthenticateAsync(string promptMessage, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable)
            return false;

#if WINDOWS
        return await AuthenticateWindowsAsync(promptMessage, cancellationToken);
#elif ANDROID
        return await AuthenticateAndroidAsync(promptMessage, cancellationToken);
#elif IOS
        return await AuthenticateIosAsync(promptMessage, cancellationToken);
#else
        return false;
#endif
    }

#if WINDOWS
    private async Task<bool> AuthenticateWindowsAsync(string promptMessage, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use Windows.Security.Credentials.UI
            var result = await Windows.Security.Credentials.UI.UserConsentVerifier
                .RequestVerificationAsync(promptMessage)
                .AsTask(cancellationToken);

            return result == Windows.Security.Credentials.UI.UserConsentVerificationResult.Verified;
        }
        catch (Exception)
        {
            return false;
        }
    }
#elif ANDROID
    private async Task<bool> AuthenticateAndroidAsync(string promptMessage, CancellationToken cancellationToken = default)
    {
        // Android implementation would require Activity context
        // TODO: Fix this in future
        try
        {
            var biometric = AndroidX.Biometric.BiometricManager.From(Platform.CurrentActivity);
            var canAuthenticate = biometric.CanAuthenticate(AndroidX.Biometric.BiometricManager.Authenticators.BiometricStrong);
            
            if (canAuthenticate != AndroidX.Biometric.BiometricManager.BiometricSuccess)
                return false;

            var tcs = new TaskCompletionSource<bool>();
            
            var prompt = new AndroidX.Biometric.BiometricPrompt(
                Platform.CurrentActivity,
                new AndroidX.Biometric.BiometricPrompt.AuthenticationCallback
                {
                    OnAuthenticationSucceeded = (sender, args) => tcs.SetResult(true),
                    OnAuthenticationFailed = (sender, args) => tcs.SetResult(false),
                    OnAuthenticationError = (sender, args) => tcs.SetResult(false)
                });

            var info = new AndroidX.Biometric.BiometricPrompt.PromptInfo.Builder()
                .SetTitle("Erebus")
                .SetSubtitle(promptMessage)
                .SetNegativeButtonText("Cancel")
                .Build();

            prompt.Authenticate(info);
            
            using (cancellationToken.Register(() => tcs.TrySetResult(false)))
            {
                return await tcs.Task;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
#endif
}
