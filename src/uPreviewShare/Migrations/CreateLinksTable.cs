using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;
using uPreviewShare.Models;

namespace uPreviewShare.Migrations;

/// <summary>
/// Creates the uPreviewShare_Links table with all indexes.
/// </summary>
public class CreateLinksTable : AsyncMigrationBase
{
    public CreateLinksTable(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync()
    {
        if (TableExists("uPreviewShare_Links"))
        {
            Logger.LogInformation("uPreviewShare_Links table already exists, skipping creation.");
            return Task.CompletedTask;
        }

        Create.Table<uPreviewShareLink>().Do();
        Logger.LogInformation("Created uPreviewShare_Links table.");
        return Task.CompletedTask;
    }
}
