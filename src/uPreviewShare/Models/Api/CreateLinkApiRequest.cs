namespace uPreviewShare.Models.Api;

/// <summary>
/// API request model for creating a new preview share link via the Management API.
/// </summary>
public class CreateLinkApiRequest
{
    /// <summary>
    /// The Umbraco content node key (GUID) to create a link for.
    /// </summary>
    public Guid NodeKey { get; set; }

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

    /// <summary>
    /// Optional culture code for variant content (e.g., "en-US", "nl").
    /// Required for culture-variant document types, optional for invariant content.
    /// </summary>
    public string? Culture { get; set; }
}
