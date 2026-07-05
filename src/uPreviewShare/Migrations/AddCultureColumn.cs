using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uPreviewShare.Migrations;

/// <summary>
/// Adds the Culture column to the uPreviewShare_Links table for variant content support.
/// </summary>
public class AddCultureColumn : AsyncMigrationBase
{
    public AddCultureColumn(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync()
    {
        if (ColumnExists("uPreviewShare_Links", "Culture"))
        {
            Logger.LogInformation("uPreviewShare_Links.Culture column already exists, skipping.");
            return Task.CompletedTask;
        }

        Alter.Table("uPreviewShare_Links")
            .AddColumn("Culture")
            .AsString(16)
            .Nullable()
            .Do();

        Logger.LogInformation("Added Culture column to uPreviewShare_Links table.");
        return Task.CompletedTask;
    }
}
