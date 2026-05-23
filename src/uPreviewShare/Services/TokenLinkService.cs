using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Scoping;
using uPreviewShare.Models;
using uPreviewShare.Models.DTOs;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Services;

/// <summary>
/// Implementation of <see cref="ITokenLinkService"/> using Umbraco's IScopeProvider/NPoco
/// for data access and IMemoryCache for cache-first token validation.
/// </summary>
public class TokenLinkService : ITokenLinkService
{
    private readonly IScopeProvider _scopeProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenLinkService> _logger;

    private const string CacheKeyPrefix = "ups:token:";
    private static readonly TimeSpan MaxCacheTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MinExpiryWindow = TimeSpan.FromMinutes(15);
    private static readonly Regex PinRegex = new(@"^\d{6}$", RegexOptions.Compiled);

    public TokenLinkService(IScopeProvider scopeProvider, IMemoryCache cache, ILogger<TokenLinkService> logger)
    {
        _scopeProvider = scopeProvider;
        _cache = cache;
        _logger = logger;
    }

    public async Task<uPreviewShareLink> CreateLinkAsync(CreateLinkRequest request, CancellationToken ct = default)
    {
        ValidateCreateRequest(request);

        var link = new uPreviewShareLink
        {
            Id = Guid.NewGuid(),
            NodeId = request.NodeId,
            Token = GenerateToken(),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            MaxViews = request.MaxViews,
            ViewCount = 0,
            PinHash = request.Pin != null ? HashPin(request.Pin) : null,
            Status = (int)LinkStatus.Active
        };

        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        await database.InsertAsync(link);
        scope.Complete();

        return link;
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return TokenValidationResult.Invalid();

        var cacheKey = $"{CacheKeyPrefix}{token}";

        try
        {
            if (_cache.TryGetValue(cacheKey, out TokenValidationResult? cached) && cached != null)
            {
                if (cached.IsValid && IsLinkExpiredOrExhausted(cached))
                {
                    _cache.Remove(cacheKey);
                    return TokenValidationResult.Invalid();
                }
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for token, falling back to database");
        }

        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var links = await database.FetchAsync<uPreviewShareLink>("SELECT * FROM uPreviewShare_Links WHERE Token = @0", new object[] { token });
        var link = links.FirstOrDefault();
        scope.Complete();

        if (link == null) return TokenValidationResult.Invalid();
        if ((LinkStatus)link.Status != LinkStatus.Active) return TokenValidationResult.Invalid();
        if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= DateTime.UtcNow) return TokenValidationResult.Invalid();
        if (link.MaxViews.HasValue && link.ViewCount >= link.MaxViews.Value) return TokenValidationResult.Invalid();

        var result = new TokenValidationResult
        {
            IsValid = true,
            LinkId = link.Id,
            NodeId = link.NodeId,
            ExpiresAt = link.ExpiresAt,
            MaxViews = link.MaxViews,
            ViewCount = link.ViewCount,
            HasPin = !string.IsNullOrEmpty(link.PinHash),
            PinHash = link.PinHash,
            Status = (LinkStatus)link.Status
        };

        PopulateCache(cacheKey, result, link.ExpiresAt);
        return result;
    }

    public async Task<int> IncrementViewCountAtomicallyAsync(Guid linkId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        await database.ExecuteAsync("UPDATE uPreviewShare_Links SET ViewCount = ViewCount + 1 WHERE Id = @0 AND Status = @1", new object[] { linkId, (int)LinkStatus.Active });
        var newViewCount = await database.ExecuteScalarAsync<int>("SELECT ViewCount FROM uPreviewShare_Links WHERE Id = @0", new object[] { linkId });
        scope.Complete();
        await EvictCacheForLinkAsync(linkId);
        return newViewCount;
    }

    public async Task RevokeLinkAsync(Guid linkId, Guid revokedBy, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var links = await database.FetchAsync<uPreviewShareLink>("SELECT * FROM uPreviewShare_Links WHERE Id = @0", new object[] { linkId });
        var link = links.FirstOrDefault();
        if (link != null && (LinkStatus)link.Status == LinkStatus.Active)
        {
            link.Status = (int)LinkStatus.Revoked;
            link.RevokedAt = DateTime.UtcNow;
            link.RevokedBy = revokedBy;
            await database.UpdateAsync(link);
            _cache.Remove($"{CacheKeyPrefix}{link.Token}");
        }
        scope.Complete();
    }

    public async Task<int> RevokeAllLinksForNodeAsync(int nodeId, Guid revokedBy, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var now = DateTime.UtcNow;

        // Get tokens for cache eviction before the bulk update
        var activeLinks = await database.FetchAsync<uPreviewShareLink>("SELECT * FROM uPreviewShare_Links WHERE NodeId = @0 AND Status = @1", new object[] { nodeId, (int)LinkStatus.Active });

        // Bulk UPDATE instead of individual updates
        var revokedCount = await database.ExecuteAsync(
            "UPDATE uPreviewShare_Links SET Status = @0, RevokedAt = @1, RevokedBy = @2 WHERE NodeId = @3 AND Status = @4",
            new object[] { (int)LinkStatus.Revoked, now, revokedBy, nodeId, (int)LinkStatus.Active });

        scope.Complete();

        // Evict cache for all affected links
        foreach (var link in activeLinks)
        {
            _cache.Remove($"{CacheKeyPrefix}{link.Token}");
        }

        return revokedCount;
    }

    public async Task<IReadOnlyList<uPreviewShareLinkDto>> GetLinksForNodeAsync(int nodeId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var links = await database.FetchAsync<uPreviewShareLink>("SELECT * FROM uPreviewShare_Links WHERE NodeId = @0 AND Status != @1 ORDER BY CreatedAt DESC", new object[] { nodeId, (int)LinkStatus.Deleted });
        scope.Complete();

        var dtos = links.Select(link => new uPreviewShareLinkDto
        {
            Id = link.Id,
            NodeId = link.NodeId,
            Token = link.Token,
            CreatedBy = link.CreatedBy,
            CreatedAt = link.CreatedAt,
            ExpiresAt = link.ExpiresAt,
            MaxViews = link.MaxViews,
            ViewCount = link.ViewCount,
            HasPin = !string.IsNullOrEmpty(link.PinHash),
            Status = (LinkStatus)link.Status,
            RevokedAt = link.RevokedAt,
            RevokedBy = link.RevokedBy
        }).ToList();

        return dtos;
    }

    public async Task<IReadOnlyList<uPreviewShareLinkDto>> GetAllLinksForNodeAsync(int nodeId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var links = await database.FetchAsync<uPreviewShareLink>("SELECT * FROM uPreviewShare_Links WHERE NodeId = @0 ORDER BY CreatedAt DESC", new object[] { nodeId });
        scope.Complete();

        var dtos = links.Select(link => new uPreviewShareLinkDto
        {
            Id = link.Id,
            NodeId = link.NodeId,
            Token = link.Token,
            CreatedBy = link.CreatedBy,
            CreatedAt = link.CreatedAt,
            ExpiresAt = link.ExpiresAt,
            MaxViews = link.MaxViews,
            ViewCount = link.ViewCount,
            HasPin = !string.IsNullOrEmpty(link.PinHash),
            Status = (LinkStatus)link.Status,
            RevokedAt = link.RevokedAt,
            RevokedBy = link.RevokedBy
        }).ToList();

        return dtos;
    }

    public async Task DeleteLinkAsync(Guid linkId, CancellationToken ct = default)
    {
        using var scope = _scopeProvider.CreateScope();
        var database = scope.Database;
        var token = await database.ExecuteScalarAsync<string>("SELECT Token FROM uPreviewShare_Links WHERE Id = @0", new object[] { linkId });
        if (!string.IsNullOrEmpty(token)) _cache.Remove($"{CacheKeyPrefix}{token}");
        await database.ExecuteAsync("UPDATE uPreviewShare_Links SET Status = @0 WHERE Id = @1", new object[] { (int)LinkStatus.Deleted, linkId });
        scope.Complete();
    }

    #region Private Helpers

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string HashPin(string pin)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        using var hmac = new HMACSHA256(salt);
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPin(string pin, string storedHash)
    {
        if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(storedHash)) return false;
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);
        using var hmac = new HMACSHA256(salt);
        var actualHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pin));
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static void ValidateCreateRequest(CreateLinkRequest request)
    {
        if (request.NodeId <= 0) throw new ArgumentException("NodeId must be a positive integer.", nameof(request));
        if (request.CreatedBy == Guid.Empty) throw new ArgumentException("CreatedBy must be a valid user ID.", nameof(request));
        if (request.ExpiresAt.HasValue)
        {
            var minimumExpiry = DateTime.UtcNow.Add(MinExpiryWindow);
            if (request.ExpiresAt.Value < minimumExpiry) throw new ArgumentException("Expiration must be at least 15 minutes in the future.", nameof(request));
        }
        if (request.MaxViews.HasValue && (request.MaxViews.Value < 1 || request.MaxViews.Value > 10000))
            throw new ArgumentException("Max views must be between 1 and 10,000.", nameof(request));
        if (request.Pin != null && !PinRegex.IsMatch(request.Pin))
            throw new ArgumentException("PIN must be exactly 6 digits (0-9).", nameof(request));
    }

    private static bool IsLinkExpiredOrExhausted(TokenValidationResult result)
    {
        if (result.ExpiresAt.HasValue && result.ExpiresAt.Value <= DateTime.UtcNow) return true;
        if (result.MaxViews.HasValue && result.ViewCount >= result.MaxViews.Value) return true;
        return false;
    }

    private void PopulateCache(string cacheKey, TokenValidationResult result, DateTime? expiresAt)
    {
        try
        {
            var ttl = MaxCacheTtl;
            if (expiresAt.HasValue)
            {
                var timeUntilExpiry = expiresAt.Value - DateTime.UtcNow;
                if (timeUntilExpiry > TimeSpan.Zero) ttl = timeUntilExpiry < MaxCacheTtl ? timeUntilExpiry : MaxCacheTtl;
                else return;
            }
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to populate cache for key {CacheKey}", cacheKey); }
    }

    private async Task EvictCacheForLinkAsync(Guid linkId)
    {
        try
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var database = scope.Database;
            var token = await database.ExecuteScalarAsync<string>("SELECT Token FROM uPreviewShare_Links WHERE Id = @0", new object[] { linkId });
            if (!string.IsNullOrEmpty(token)) _cache.Remove($"{CacheKeyPrefix}{token}");
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to evict cache for link {LinkId}", linkId); }
    }

    #endregion
}
