using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace uPreviewShare.Models;

/// <summary>
/// Represents the branding configuration for the Preview_Page and PIN_Screen.
/// Maps to the uPreviewShare_Branding database table.
/// </summary>
[TableName("uPreviewShare_Branding")]
[PrimaryKey("Id", AutoIncrement = false)]
public class uPreviewShareBrandingConfig
{
    /// <summary>
    /// Unique identifier for the branding configuration.
    /// </summary>
    [PrimaryKeyColumn(AutoIncrement = false)]
    [Column("Id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The content node ID this branding applies to.
    /// Null means this is the global branding configuration.
    /// A non-null value indicates a per-page branding override.
    /// </summary>
    [Column("NodeId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? NodeId { get; set; }

    /// <summary>
    /// The relative path to the uploaded logo image.
    /// Null means default branding is used.
    /// </summary>
    [Column("LogoPath")]
    [Length(256)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? LogoPath { get; set; }

    /// <summary>
    /// The primary color as a 6-digit hex value (e.g., "#8B5CF6").
    /// Null means the default primary color is used.
    /// </summary>
    [Column("PrimaryColor")]
    [Length(7)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// The background color as a 6-digit hex value (e.g., "#f8fafc").
    /// Null means the default background color is used.
    /// </summary>
    [Column("BackgroundColor")]
    [Length(7)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// The text color as a 6-digit hex value (e.g., "#1e293b").
    /// Null means the default text color is used.
    /// </summary>
    [Column("TextColor")]
    [Length(7)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? TextColor { get; set; }

    /// <summary>
    /// The UTC timestamp when the branding was last updated.
    /// </summary>
    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The user ID of the content editor who last updated the branding.
    /// </summary>
    [Column("UpdatedBy")]
    public Guid UpdatedBy { get; set; }
}
