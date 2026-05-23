using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using uPreviewShare.Services;

namespace uPreviewShare.Notifications;

/// <summary>
/// Handles content deleted notifications to automatically revoke
/// all active uPreviewShare links for the affected nodes.
/// </summary>
public class uPreviewShareContentNotificationHandler
    : INotificationAsyncHandler<ContentDeletedNotification>
{
    private readonly ITokenLinkService _tokenLinkService;
    private readonly ILogger<uPreviewShareContentNotificationHandler> _logger;
    private static readonly Guid SystemUserId = Guid.Empty;

    public uPreviewShareContentNotificationHandler(ITokenLinkService tokenLinkService, ILogger<uPreviewShareContentNotificationHandler> logger)
    {
        _tokenLinkService = tokenLinkService;
        _logger = logger;
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var content in notification.DeletedEntities)
            {
                try
                {
                    var revokedCount = await _tokenLinkService.RevokeAllLinksForNodeAsync(content.Id, SystemUserId, cancellationToken);
                    if (revokedCount > 0)
                        _logger.LogInformation("Content deleted: Revoked {Count} active uPreviewShare link(s) for node {NodeId}.", revokedCount, content.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to revoke uPreviewShare links for deleted node {NodeId}. Orphaned links will be cleaned up by the background service.", content.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "uPreviewShare notification handler encountered an unexpected error. Content deletion will proceed normally.");
        }
    }
}
