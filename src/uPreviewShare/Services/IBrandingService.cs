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
}
