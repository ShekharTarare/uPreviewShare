using System.ComponentModel.DataAnnotations;

namespace uPreviewShare.Models.Api;

/// <summary>
/// API request model for updating branding colors via the Management API.
/// </summary>
public class UpdateBrandingRequest
{
    /// <summary>
    /// The primary color as a 6-digit hex value (e.g., "#8B5CF6"), or null to keep current.
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #8B5CF6)")]
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// The background color as a 6-digit hex value (e.g., "#f8fafc"), or null to keep current.
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #f8fafc)")]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// The text color as a 6-digit hex value (e.g., "#1e293b"), or null to keep current.
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #ffffff)")]
    public string? TextColor { get; set; }
}
