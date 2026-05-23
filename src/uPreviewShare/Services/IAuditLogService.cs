using uPreviewShare.Models.DTOs;

namespace uPreviewShare.Services;

/// <summary>
/// Service for recording and querying audit log entries for preview share link events.
/// </summary>
public interface IAuditLogService
{
    Task LogAccessAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default);
    Task LogFailedPinAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default);
    Task LogRevocationAsync(Guid linkId, int nodeId, Guid revokedBy, CancellationToken ct = default);
    Task LogLockoutAsync(Guid linkId, string ipAddress, string userAgent, CancellationToken ct = default);
    Task<PagedResult<AuditLogEntryDto>> GetLogsForNodeAsync(int nodeId, int page, int pageSize = 50, string? eventType = null, Guid? linkId = null, CancellationToken ct = default);
}
