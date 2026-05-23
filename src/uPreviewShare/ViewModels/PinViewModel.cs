using uPreviewShare.Models.DTOs;

namespace uPreviewShare.ViewModels;

public class PinViewModel
{
    public string Token { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int RemainingAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public int RetryAfterSeconds { get; set; }
    public BrandingConfigDto? BrandingConfig { get; set; }
}
