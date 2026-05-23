namespace uPreviewShare.Models;

/// <summary>
/// Represents a sliding window rate limit entry for tracking failed PIN attempts.
/// </summary>
public class SlidingWindowEntry
{
    /// <summary>
    /// Queue of timestamps for failed attempts within the sliding window.
    /// </summary>
    public Queue<DateTime> Attempts { get; } = new();

    /// <summary>
    /// If set, the entry is locked out until this UTC time.
    /// </summary>
    public DateTime? LockedUntil { get; set; }
}
