namespace uPreviewShare.Models.Enums;

/// <summary>
/// Represents the current status of a preview share link.
/// </summary>
public enum LinkStatus
{
    /// <summary>
    /// The link is active and can be accessed.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The link has been manually revoked by a content editor.
    /// </summary>
    Revoked = 1,

    /// <summary>
    /// The link has expired (past its expiration timestamp or max views reached).
    /// </summary>
    Expired = 2,

    /// <summary>
    /// The link has been soft-deleted. The link row is retained for audit trail purposes
    /// but is no longer visible in the links list or accessible via token.
    /// </summary>
    Deleted = 3
}
