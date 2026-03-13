namespace Erebus.Core;

/// <summary>
/// Application settings stored in settings.db
/// </summary>
public sealed class AppSettings
{
    public AppSettings()
    {
        Theme = AppTheme.System;
        SessionTimeoutMinutes = 15;
        AutoBackupEnabled = false;
        BackupIntervalDays = 7;
        ShowFavicons = true;
        EnableAnimations = true;
    }

    /// <summary>
    /// UI theme preference
    /// </summary>
    public AppTheme Theme { get; set; }

    /// <summary>
    /// Session timeout (0 = never)
    /// </summary>
    public int SessionTimeoutMinutes { get; set; }
    public bool AutoBackupEnabled { get; set; }
    public int BackupIntervalDays { get; set; }
    public bool ShowFavicons { get; set; }
    public bool EnableAnimations { get; set; }
    public string? LastOpenedVaultId { get; set; }
    public bool UseBiometric { get; set; }

    /// <summary>
    /// PIN code for quick unlock
    /// </summary>
    public string? PinHash { get; set; }
}
