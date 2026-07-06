using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace uPreviewShare.Migrations;

/// <summary>
/// Adds the nullable NodeId column to the uPreviewShare_Branding table
/// and creates a filtered unique index to prevent duplicate per-page overrides.
/// </summary>
public class AddBrandingNodeIdColumn : AsyncMigrationBase
{
    public AddBrandingNodeIdColumn(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync()
    {
        // 1. Handle the column independently
        if (!ColumnExists("uPreviewShare_Branding", "NodeId"))
        {
            Create.Column("NodeId")
                .OnTable("uPreviewShare_Branding")
                .AsInt32()
                .Nullable()
                .Do();

            Logger.LogInformation("Added NodeId column to uPreviewShare_Branding table.");
        }
        else
        {
            Logger.LogInformation("uPreviewShare_Branding.NodeId column already exists, skipping column creation.");
        }

        // 2. Handle the index independently
        const string indexName = "IX_uPreviewShare_Branding_NodeId";
        if (!IndexExists(indexName))
        {
            // SQLite treats multiple NULLs as distinct by default.
            // SQL Server requires a filtered index to allow multiple NULLs.
            var isSqlite = SqlSyntax.GetType().Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase);

            if (isSqlite)
            {
                Execute.Sql(
                    $"CREATE UNIQUE INDEX [{indexName}] " +
                    "ON [uPreviewShare_Branding] ([NodeId]);").Do();
            }
            else
            {
                Execute.Sql(
                    $"CREATE UNIQUE INDEX [{indexName}] " +
                    "ON [uPreviewShare_Branding] ([NodeId]) " +
                    "WHERE [NodeId] IS NOT NULL;").Do();
            }

            Logger.LogInformation("Added unique index {IndexName} to uPreviewShare_Branding table.", indexName);
        }
        else
        {
            Logger.LogInformation("Unique index {IndexName} already exists, skipping index creation.", indexName);
        }

        return Task.CompletedTask;
    }
}
