using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;
using uPreviewShare.Models;

namespace uPreviewShare.Migrations;

/// <summary>
/// Creates the uPreviewShare_Branding table (includes TextColor column).
/// </summary>
public class CreateBrandingTable : AsyncMigrationBase
{
    public CreateBrandingTable(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync()
    {
        if (TableExists("uPreviewShare_Branding"))
        {
            Logger.LogInformation("uPreviewShare_Branding table already exists, skipping creation.");
            return Task.CompletedTask;
        }

        Create.Table<uPreviewShareBrandingConfig>().Do();
        Logger.LogInformation("Created uPreviewShare_Branding table.");
        return Task.CompletedTask;
    }
}
