using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Models;

/// <summary>
/// Represents a preview sharing link entity.
/// Maps to the uPreviewShare_Links database table.
/// </summary>
[TableName("uPreviewShare_Links")]
[PrimaryKey("Id", AutoIncrement = false)]
public class uPreviewShareLink
{
    /// <summary>
    /// Unique identifier for the link.
    /// </summary>
    [PrimaryKeyColumn(AutoIncrement = false)]
    [Column("Id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The Umbraco content node ID this link is associated with.
    /// </summary>
    [Column("NodeId")]
    [Index(IndexTypes.NonClustered, Name = "IX_uPreviewShare_Links_NodeId")]
    public int NodeId { get; set; }

    /// <summary>
    /// The cryptographically random token used in the share URL.
    /// Must be unique across all links.
    /// </summary>
    [Column("Token")]
    [Length(64)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_uPreviewShare_Links_Token")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID of the content editor who created this link.
    /// </summary>
    [Column("CreatedBy")]
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// The UTC timestamp when this link was created.
    /// </summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The optional UTC timestamp when this link expires.
    /// Null means the link does not expire based on time.
    /// </summary>
    [Column("ExpiresAt")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// The optional maximum number of views allowed for this link.
    /// Null means unlimited views.
    /// </summary>
    [Column("MaxViews")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? MaxViews { get; set; }

    /// <summary>
    /// The current number of times this link has been accessed.
    /// </summary>
    [Column("ViewCount")]
    public int ViewCount { get; set; }

    /// <summary>
    /// The hashed PIN for PIN-protected links.
    /// Null means the link is not PIN-protected.
    /// </summary>
    [Column("PinHash")]
    [Length(128)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? PinHash { get; set; }

    /// <summary>
    /// The current status of the link.
    /// </summary>
    [Column("Status")]
    [Index(IndexTypes.NonClustered, ForColumns = "Status,ExpiresAt", Name = "IX_uPreviewShare_Links_Status_ExpiresAt")]
    public int Status { get; set; }

    /// <summary>
    /// The optional UTC timestamp when this link was revoked.
    /// </summary>
    [Column("RevokedAt")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// The optional user ID of the content editor who revoked this link.
    /// </summary>
    [Column("RevokedBy")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public Guid? RevokedBy { get; set; }

    /// <summary>
    /// The optional culture/language code for variant content (e.g., "en-US", "nl").
    /// Null means invariant content or default culture.
    /// </summary>
    [Column("Culture")]
    [Length(16)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Culture { get; set; }
}
