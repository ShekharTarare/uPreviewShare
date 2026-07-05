using System.Text.Json.Serialization;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Data transfer object for displaying link information in the backoffice.
/// </summary>
public class uPreviewShareLinkDto
{
    /// <summary>
    /// The unique link identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The associated content node ID.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// The token value used in the share URL.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID of the creator.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When the link was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the link expires (null = no expiry).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Maximum views allowed (null = unlimited).
    /// </summary>
    public int? MaxViews { get; set; }

    /// <summary>
    /// Current view count.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Whether the link is PIN-protected.
    /// </summary>
    public bool HasPin { get; set; }

    /// <summary>
    /// The current status of the link.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LinkStatus Status { get; set; }

    /// <summary>
    /// When the link was revoked (null if not revoked).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Who revoked the link (null if not revoked).
    /// </summary>
    public Guid? RevokedBy { get; set; }

    /// <summary>
    /// The culture/language code for variant content (null for invariant).
    /// </summary>
    public string? Culture { get; set; }
}
