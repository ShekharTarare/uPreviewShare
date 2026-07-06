namespace uPreviewShare.Models.DTOs;

/// <summary>
/// Data transfer object for branding configuration API responses.
/// </summary>
public class BrandingConfigDto
{
    /// <summary>
    /// The relative path to the uploaded logo image, or null if using default branding.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>
    /// The primary color as a 6-digit hex value (e.g., "#8B5CF6"), or null if using default.
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// The background color as a 6-digit hex value (e.g., "#f8fafc"), or null if using default.
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// The text color as a 6-digit hex value (e.g., "#1e293b"), or null if using default.
    /// </summary>
    public string? TextColor { get; set; }

    /// <summary>
    /// The UTC timestamp when the branding was last updated, or null if no branding is configured.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether custom branding is configured (true) or defaults are in use (false).
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// The content node ID this branding belongs to, or null if this is the global branding configuration.
    /// </summary>
    public int? NodeId { get; set; }

    /// <summary>
    /// Whether this branding configuration is a per-page override (true) or the global default (false).
    /// </summary>
    public bool IsOverride { get; set; }
}
