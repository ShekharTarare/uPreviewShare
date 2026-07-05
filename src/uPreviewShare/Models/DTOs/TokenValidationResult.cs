using uPreviewShare.Models.Enums;

namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Result of validating a token, containing the link details if valid.
/// </summary>
public class TokenValidationResult
{
    /// <summary>
    /// Whether the token is valid and the link is accessible.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The link ID if the token is valid.
    /// </summary>
    public Guid? LinkId { get; set; }

    /// <summary>
    /// The associated node ID if the token is valid.
    /// </summary>
    public int? NodeId { get; set; }

    /// <summary>
    /// The expiration timestamp if set.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// The maximum views allowed if set.
    /// </summary>
    public int? MaxViews { get; set; }

    /// <summary>
    /// The current view count.
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Whether the link requires a PIN.
    /// </summary>
    public bool HasPin { get; set; }

    /// <summary>
    /// The hashed PIN value (for verification by the caller). Not exposed externally.
    /// </summary>
    public string? PinHash { get; set; }

    /// <summary>
    /// The link status.
    /// </summary>
    public LinkStatus Status { get; set; }

    /// <summary>
    /// The culture code for variant content (null for invariant).
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Creates an invalid result (used for non-existent, expired, or revoked tokens).
    /// </summary>
    public static TokenValidationResult Invalid() => new() { IsValid = false };
}
