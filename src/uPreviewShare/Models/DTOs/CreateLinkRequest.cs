namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Request model for creating a new preview share link.
/// </summary>
public class CreateLinkRequest
{
    /// <summary>
    /// The Umbraco content node ID to create a link for.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// The user ID of the content editor creating the link.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Optional expiration timestamp. Must be at least 15 minutes in the future if specified.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Optional maximum number of views (1-10000). Null means unlimited views.
    /// </summary>
    public int? MaxViews { get; set; }

    /// <summary>
    /// Optional 6-digit numeric PIN for PIN-protected links. Must be exactly 6 digits (0-9).
    /// </summary>
    public string? Pin { get; set; }
}
