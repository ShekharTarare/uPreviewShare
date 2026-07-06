using Umbraco.Cms.Core.Packaging;

namespace uPreviewShare.Migrations;

/// <summary>
/// Defines the migration plan for the uPreviewShare package.
/// Migrations are executed in order on application startup.
/// </summary>
public class uPreviewShareMigrationPlan : PackageMigrationPlan
{
    public uPreviewShareMigrationPlan()
        : base("uPreviewShare")
    {
    }

    protected override void DefinePlan()
    {
        From(string.Empty)
            .To<CreateLinksTable>("create-upreviewshare-links-table")
            .To<CreateAuditLogTable>("create-upreviewshare-auditlog-table")
            .To<CreateBrandingTable>("create-upreviewshare-branding-table")
            .To<AddCultureColumn>("add-upreviewshare-culture-column")
            .To<AddBrandingNodeIdColumn>("add-upreviewshare-branding-nodeid-column");
    }
}
