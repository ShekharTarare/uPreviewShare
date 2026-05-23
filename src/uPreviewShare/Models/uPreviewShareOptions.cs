namespace uPreviewShare.Models;

/// <summary>
/// Configuration options for uPreviewShare, bindable from appsettings.json section "uPreviewShare".
/// </summary>
public class uPreviewShareOptions
{
    public const string SectionName = "uPreviewShare";

    /// <summary>Maximum failed PIN attempts before lockout. Default: 5.</summary>
    public int MaxPinAttempts { get; set; } = 5;

    /// <summary>Lockout duration in minutes after max failed attempts. Default: 15.</summary>
    public int LockoutDurationMinutes { get; set; } = 15;

    /// <summary>Sliding window duration in minutes for tracking attempts. Default: 15.</summary>
    public int AttemptWindowMinutes { get; set; } = 15;

    /// <summary>PIN session cookie duration in minutes. Default: 30.</summary>
    public int SessionDurationMinutes { get; set; } = 30;

    /// <summary>Interval in minutes for the expired link cleanup background service. Default: 5.</summary>
    public int CleanupIntervalMinutes { get; set; } = 5;
}
