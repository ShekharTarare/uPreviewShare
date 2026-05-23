using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Models;

/// <summary>
/// Represents an audit log entry for preview share link access events.
/// Maps to the uPreviewShare_AuditLog database table.
/// </summary>
[TableName("uPreviewShare_AuditLog")]
[PrimaryKey("Id", AutoIncrement = false)]
public class uPreviewShareAuditLogEntry
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    [PrimaryKeyColumn(AutoIncrement = false)]
    [Column("Id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the link this audit entry is associated with.
    /// </summary>
    [Column("LinkId")]
    [Index(IndexTypes.NonClustered, Name = "IX_uPreviewShare_AuditLog_LinkId")]
    public Guid LinkId { get; set; }

    /// <summary>
    /// The type of event that was recorded.
    /// </summary>
    [Column("EventType")]
    public int EventType { get; set; }

    /// <summary>
    /// The UTC timestamp when the event occurred, with millisecond precision.
    /// </summary>
    [Column("Timestamp")]
    [Index(IndexTypes.NonClustered, Name = "IX_uPreviewShare_AuditLog_Timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The IP address of the visitor (supports IPv4 and IPv6, max 45 characters).
    /// </summary>
    [Column("IpAddress")]
    [Length(45)]
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// The user-agent string of the visitor's browser, truncated to 512 characters.
    /// </summary>
    [Column("UserAgent")]
    [Length(512)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? UserAgent { get; set; }
}
