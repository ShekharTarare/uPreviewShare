using System.Text.Json.Serialization;
using uPreviewShare.Models.Enums;

namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Data transfer object representing an audit log entry for API responses.
/// </summary>
public class AuditLogEntryDto
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The ID of the link this audit entry is associated with.
    /// </summary>
    public Guid LinkId { get; set; }

    /// <summary>
    /// The type of event that was recorded.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuditEventType EventType { get; set; }

    /// <summary>
    /// The UTC timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The IP address of the visitor.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// The user-agent string of the visitor's browser (truncated to 512 characters).
    /// </summary>
    public string? UserAgent { get; set; }
}
