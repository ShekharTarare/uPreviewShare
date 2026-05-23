using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using uPreviewShare.Models;

namespace uPreviewShare.Services;

/// <summary>
/// In-memory sliding window rate limiter for PIN attempts.
/// Registered as a singleton to maintain state across requests.
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly uPreviewShareOptions _options;

    private readonly ConcurrentDictionary<string, SlidingWindowEntry> _entries = new();

    public RateLimitService(IOptions<uPreviewShareOptions> options)
    {
        _options = options.Value;
    }

    public bool IsLockedOut(string ipAddress, Guid linkId)
    {
        var key = BuildKey(ipAddress, linkId);
        if (!_entries.TryGetValue(key, out var entry)) return false;
        lock (entry) { return entry.LockedUntil.HasValue && entry.LockedUntil.Value > DateTime.UtcNow; }
    }

    public int RecordFailedAttempt(string ipAddress, Guid linkId)
    {
        var key = BuildKey(ipAddress, linkId);
        var entry = _entries.GetOrAdd(key, _ => new SlidingWindowEntry());
        lock (entry)
        {
            var now = DateTime.UtcNow;
            if (entry.LockedUntil.HasValue && entry.LockedUntil.Value > now) return 0;
            PruneWindowEntries(entry, now);
            entry.Attempts.Enqueue(now);
            if (entry.Attempts.Count >= _options.MaxPinAttempts) { entry.LockedUntil = now.Add(TimeSpan.FromMinutes(_options.LockoutDurationMinutes)); return 0; }
            return _options.MaxPinAttempts - entry.Attempts.Count;
        }
    }

    public void ResetAttempts(string ipAddress, Guid linkId)
    {
        var key = BuildKey(ipAddress, linkId);
        if (_entries.TryGetValue(key, out var entry)) { lock (entry) { entry.Attempts.Clear(); entry.LockedUntil = null; } }
    }

    public int GetRemainingAttempts(string ipAddress, Guid linkId)
    {
        var key = BuildKey(ipAddress, linkId);
        if (!_entries.TryGetValue(key, out var entry)) return _options.MaxPinAttempts;
        lock (entry)
        {
            var now = DateTime.UtcNow;
            if (entry.LockedUntil.HasValue && entry.LockedUntil.Value > now) return 0;
            PruneWindowEntries(entry, now);
            return Math.Max(0, _options.MaxPinAttempts - entry.Attempts.Count);
        }
    }

    public void PruneExpiredEntries()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _entries)
        {
            var entry = kvp.Value;
            bool shouldRemove;
            lock (entry)
            {
                PruneWindowEntries(entry, now);
                var lockoutExpired = !entry.LockedUntil.HasValue || entry.LockedUntil.Value <= now;
                shouldRemove = lockoutExpired && entry.Attempts.Count == 0;
            }
            if (shouldRemove) _entries.TryRemove(kvp.Key, out _);
        }
    }

    public TimeSpan? GetLockoutRemaining(string ipAddress, Guid linkId)
    {
        var key = BuildKey(ipAddress, linkId);
        if (!_entries.TryGetValue(key, out var entry)) return null;
        lock (entry)
        {
            if (!entry.LockedUntil.HasValue) return null;
            var remaining = entry.LockedUntil.Value - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : null;
        }
    }

    private static string BuildKey(string ipAddress, Guid linkId) => $"{ipAddress}:{linkId}";

    private void PruneWindowEntries(SlidingWindowEntry entry, DateTime now)
    {
        var windowStart = now - TimeSpan.FromMinutes(_options.AttemptWindowMinutes);
        while (entry.Attempts.Count > 0 && entry.Attempts.Peek() < windowStart) entry.Attempts.Dequeue();
    }
}

