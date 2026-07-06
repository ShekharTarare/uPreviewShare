using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Scoping;
using uPreviewShare.Models;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Services;

/// <summary>
/// Background job that periodically marks expired preview links and prunes rate-limit entries.
/// Uses Umbraco's IRecurringBackgroundJob for proper scope and lifecycle management.
/// </summary>
public class ExpiredLinkCleanupService : IRecurringBackgroundJob
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IMemoryCache _cache;
    private readonly IRateLimitService _rateLimitService;
    private readonly ILogger<ExpiredLinkCleanupService> _logger;

    private const string CacheKeyPrefix = "ups:token:";

    public TimeSpan Period { get; }
    public TimeSpan Delay => TimeSpan.FromMinutes(1);

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    public ExpiredLinkCleanupService(
        IScopeProvider scopeProvider,
        IMemoryCache cache,
        IRateLimitService rateLimitService,
        IOptions<uPreviewShareOptions> options,
        ILogger<ExpiredLinkCleanupService> logger)
    {
        _scopeProvider = scopeProvider;
        _cache = cache;
        _rateLimitService = rateLimitService;
        _logger = logger;
        Period = TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes);
    }

    public async Task RunJobAsync()
    {
        var expiredCount = 0;
        var expiredTokens = new List<string>();

        try
        {
            using (var dbScope = _scopeProvider.CreateScope(autoComplete: true))
            {
                var database = dbScope.Database;
                var now = DateTime.UtcNow;

                var tokens = await database.FetchAsync<string>(
                    "SELECT Token FROM uPreviewShare_Links WHERE Status = @0 AND ExpiresAt IS NOT NULL AND ExpiresAt < @1",
                    new object[] { (int)LinkStatus.Active, now });

                if (tokens is { Count: > 0 })
                {
                    expiredTokens.AddRange(tokens);
                    expiredCount = await database.ExecuteAsync(
                        "UPDATE uPreviewShare_Links SET Status = @0 WHERE Status = @1 AND ExpiresAt IS NOT NULL AND ExpiresAt < @2",
                        new object[] { (int)LinkStatus.Expired, (int)LinkStatus.Active, now });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired links from database.");
            return;
        }

        foreach (var token in expiredTokens)
            _cache.Remove($"{CacheKeyPrefix}{token}");

        try { _rateLimitService.PruneExpiredEntries(); }
        catch (Exception ex) { _logger.LogError(ex, "Failed to prune stale rate-limit entries."); }

        _logger.LogInformation("Cleanup completed: {ExpiredCount} links marked as expired, rate-limit entries pruned.", expiredCount);
    }
}
