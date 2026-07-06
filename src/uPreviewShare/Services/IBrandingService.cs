using uPreviewShare.Models.DTOs;

namespace uPreviewShare.Services;

/// <summary>
/// Service for managing branding configuration (logo, colors) for the Preview_Page and PIN_Screen.
/// </summary>
public interface IBrandingService
{
    Task<BrandingConfigDto> GetBrandingAsync(CancellationToken ct = default);
    Task SaveBrandingAsync(string? primaryColor, string? backgroundColor, string? textColor, CancellationToken ct = default);
    Task<string> SaveLogoAsync(Stream fileStream, string fileName, long fileSize, CancellationToken ct = default);
    Task ResetBrandingAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the resolved branding configuration for a specific content node.
    /// Implements a fallback chain: page override → global → defaults.
    /// </summary>
    /// <param name="nodeId">The content node ID to resolve branding for, or <c>null</c> to retrieve global branding.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved <see cref="BrandingConfigDto"/> for the given node.</returns>
    Task<BrandingConfigDto> GetBrandingAsync(int? nodeId, CancellationToken ct = default);

    /// <summary>
    /// Saves branding color configuration for a specific content node or the global branding.
    /// </summary>
    /// <param name="primaryColor">The primary color in hex format (e.g., #8B5CF6), or <c>null</c> to clear.</param>
    /// <param name="backgroundColor">The background color in hex format, or <c>null</c> to clear.</param>
    /// <param name="textColor">The text color in hex format, or <c>null</c> to clear.</param>
    /// <param name="nodeId">The content node ID to save the override for, or <c>null</c> to save global branding.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveBrandingAsync(string? primaryColor, string? backgroundColor, string? textColor, int? nodeId, CancellationToken ct = default);

    /// <summary>
    /// Saves a logo image for a specific content node or the global branding.
    /// </summary>
    /// <param name="fileStream">The logo file stream.</param>
    /// <param name="fileName">The original file name including extension.</param>
    /// <param name="fileSize">The file size in bytes.</param>
    /// <param name="nodeId">The content node ID to save the logo for, or <c>null</c> to save for global branding.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The relative path where the logo was saved.</returns>
    Task<string> SaveLogoAsync(Stream fileStream, string fileName, long fileSize, int? nodeId, CancellationToken ct = default);

    /// <summary>
    /// Resets (deletes) the branding override for a specific content node, or resets global branding.
    /// When a per-page override is reset, subsequent branding resolution for that node falls back to global branding.
    /// </summary>
    /// <param name="nodeId">The content node ID to reset the override for, or <c>null</c> to reset global branding.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ResetBrandingAsync(int? nodeId, CancellationToken ct = default);
}
