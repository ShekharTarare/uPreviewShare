using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Infrastructure.Scoping;
using uPreviewShare.Models;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Services;

public class ExpiredLinkCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly IRateLimitService _rateLimitService;
    private readonly ILogger<ExpiredLinkCleanupService> _logger;

    private const string CacheKeyPrefix = "ups:token:";
    private readonly TimeSpan _cleanupInterval;

    public ExpiredLinkCleanupService(IServiceProvider serviceProvider, IMemoryCache cache, IRateLimitService rateLimitService, IOptions<uPreviewShareOptions> options, ILogger<ExpiredLinkCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _rateLimitService = rateLimitService;
        _cleanupInterval = TimeSpan.FromMinutes(options.Value.CleanupIntervalMinutes);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredLinkCleanupService started. Running every {Interval} minutes.", _cleanupInterval.TotalMinutes);
        using var timer = new PeriodicTimer(_cleanupInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await CleanupExpiredLinksAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Error during expired link cleanup. Will retry on next cycle."); }
        }
        _logger.LogInformation("ExpiredLinkCleanupService stopped.");
    }

    private async Task CleanupExpiredLinksAsync(CancellationToken ct)
    {
        var expiredCount = 0;
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scopeProvider = scope.ServiceProvider.GetRequiredService<IScopeProvider>();
            using var dbScope = scopeProvider.CreateScope();
            var database = dbScope.Database;
            var now = DateTime.UtcNow;
            var expiredTokens = await database.FetchAsync<string>("SELECT Token FROM uPreviewShare_Links WHERE Status = @0 AND ExpiresAt IS NOT NULL AND ExpiresAt < @1", new object[] { (int)LinkStatus.Active, now });

            if (expiredTokens.Count > 0)
            {
                expiredCount = await database.ExecuteAsync(
                    "UPDATE uPreviewShare_Links SET Status = @0 WHERE Status = @1 AND ExpiresAt IS NOT NULL AND ExpiresAt < @2",
                    new object[] { (int)LinkStatus.Expired, (int)LinkStatus.Active, now });
                foreach (var token in expiredTokens)
                    _cache.Remove($"{CacheKeyPrefix}{token}");
            }
            dbScope.Complete();
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to cleanup expired links from database."); }

        var prunedBefore = 0;
        try { _rateLimitService.PruneExpiredEntries(); prunedBefore = 1; }
        catch (Exception ex) { _logger.LogError(ex, "Failed to prune stale rate-limit entries."); }

        if (expiredCount > 0 || prunedBefore > 0)
            _logger.LogInformation("Cleanup completed: {ExpiredCount} links marked as expired, rate-limit entries pruned.", expiredCount);
    }
}
