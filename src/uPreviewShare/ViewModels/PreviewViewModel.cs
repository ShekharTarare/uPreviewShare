using uPreviewShare.Models.DTOs;

namespace uPreviewShare.ViewModels;

public class PreviewViewModel
{
    public int NodeId { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public BrandingConfigDto? BrandingConfig { get; set; }
    public bool IsDraft { get; set; }
}
