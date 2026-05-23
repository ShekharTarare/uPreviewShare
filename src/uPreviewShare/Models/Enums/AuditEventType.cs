namespace uPreviewShare.Models.Enums;

/// <summary>
/// Represents the type of event recorded in the audit log.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// A successful access/view of the draft content.
    /// </summary>
    Access = 0,

    /// <summary>
    /// A failed PIN entry attempt.
    /// </summary>
    FailedPin = 1,

    /// <summary>
    /// A link revocation event.
    /// </summary>
    Revocation = 2,

    /// <summary>
    /// An IP address was locked out due to too many failed PIN attempts.
    /// </summary>
    Lockout = 3
}
