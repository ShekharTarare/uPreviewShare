namespace uPreviewShare.Services;

/// <summary>
/// In-memory rate limiting service for PIN attempts.
/// Uses a sliding window counter per IP address and link ID combination.
/// </summary>
public interface IRateLimitService
{
    bool IsLockedOut(string ipAddress, Guid linkId);
    int RecordFailedAttempt(string ipAddress, Guid linkId);
    void ResetAttempts(string ipAddress, Guid linkId);
    int GetRemainingAttempts(string ipAddress, Guid linkId);
    void PruneExpiredEntries();
    TimeSpan? GetLockoutRemaining(string ipAddress, Guid linkId);
}
