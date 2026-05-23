using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Scoping;
using uPreviewShare.Models;
using uPreviewShare.Models.DTOs;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Services;

/// <summary>
/// Implementation of <see cref="IAuditLogService"/> using Umbraco's IScopeProvider
/// and NPoco for database access.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private const int MaxUserAgentLength = 512;
    private const int MaxPageSize = 50;

    private readonly IScopeProvider _scopeProvider;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IScopeProvider scopeProvider, ILogger<AuditLogService> logger)
    {
        _scopeProvider = scopeProvider;
        _logger = logger;
    }

    public async Task LogAccessAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default)
    {
        var entry = CreateEntry(linkId, (int)AuditEventType.Access, ipAddress, userAgent);
        await PersistEntryAsync(entry, ct);
    }

    public async Task LogFailedPinAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default)
    {
        var entry = CreateEntry(linkId, (int)AuditEventType.FailedPin, ipAddress, userAgent);
        await PersistEntryAsync(entry, ct);
    }

    public async Task LogRevocationAsync(Guid linkId, int nodeId, Guid revokedBy, CancellationToken ct = default)
    {
        var entry = new uPreviewShareAuditLogEntry
        {
            Id = Guid.NewGuid(),
            LinkId = linkId,
            EventType = (int)AuditEventType.Revocation,
            Timestamp = DateTime.UtcNow,
            IpAddress = string.Empty,
            UserAgent = $"RevokedBy:{revokedBy}"
        };

        await PersistEntryAsync(entry, ct);
    }

    public async Task LogLockoutAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default)
    {
        var entry = CreateEntry(linkId, (int)AuditEventType.Lockout, ipAddress, userAgent);
        await PersistEntryAsync(entry, ct);
    }

    public async Task<PagedResult<AuditLogEntryDto>> GetLogsForNodeAsync(int nodeId, int page, int pageSize = 50, string? eventType = null, Guid? linkId = null, CancellationToken ct = default)
    {
        pageSize = Math.Min(pageSize, MaxPageSize);
        if (page < 1) page = 1;

        using var scope = _scopeProvider.CreateScope();
        var db = scope.Database;

        var sql = @"SELECT al.* FROM uPreviewShare_AuditLog al
              INNER JOIN uPreviewShare_Links l ON al.LinkId = l.Id
              WHERE l.NodeId = @0";

        var parameters = new List<object> { nodeId };
        var paramIndex = 1;

        if (!string.IsNullOrEmpty(eventType) && Enum.TryParse<AuditEventType>(eventType, true, out var parsedEventType))
        {
            sql += $" AND al.EventType = @{paramIndex}";
            parameters.Add((int)parsedEventType);
            paramIndex++;
        }

        if (linkId.HasValue)
        {
            sql += $" AND al.LinkId = @{paramIndex}";
            parameters.Add(linkId.Value);
            paramIndex++;
        }

        sql += " ORDER BY al.[Timestamp] DESC";

        var pagedData = await db.PageAsync<uPreviewShareAuditLogEntry>(
            page,
            pageSize,
            sql,
            parameters.ToArray());

        scope.Complete();

        var items = pagedData.Items.Select(e => new AuditLogEntryDto
        {
            Id = e.Id,
            LinkId = e.LinkId,
            EventType = (AuditEventType)e.EventType,
            Timestamp = e.Timestamp,
            IpAddress = e.IpAddress,
            UserAgent = e.UserAgent
        }).ToList();

        return new PagedResult<AuditLogEntryDto>
        {
            Items = items,
            TotalItems = pagedData.TotalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    private uPreviewShareAuditLogEntry CreateEntry(Guid linkId, int eventType, string ipAddress, string userAgent)
    {
        return new uPreviewShareAuditLogEntry
        {
            Id = Guid.NewGuid(),
            LinkId = linkId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = TruncateUserAgent(userAgent)
        };
    }

    private async Task PersistEntryAsync(uPreviewShareAuditLogEntry entry, CancellationToken ct = default)
    {
        try
        {
            using var scope = _scopeProvider.CreateScope();
            var db = scope.Database;
            await db.InsertAsync(entry);
            scope.Complete();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist audit log entry {EntryId} for link {LinkId}", entry.Id, entry.LinkId);
            throw;
        }
    }

    private static string? TruncateUserAgent(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return userAgent;

        return userAgent.Length > MaxUserAgentLength
            ? userAgent[..MaxUserAgentLength]
            : userAgent;
    }
}
