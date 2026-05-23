using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;
using uPreviewShare.Models;

namespace uPreviewShare.Migrations;

/// <summary>
/// Creates the uPreviewShare_AuditLog table with all indexes.
/// </summary>
public class CreateAuditLogTable : AsyncMigrationBase
{
    public CreateAuditLogTable(IMigrationContext context) : base(context) { }

    protected override Task MigrateAsync()
    {
        if (TableExists("uPreviewShare_AuditLog"))
        {
            Logger.LogInformation("uPreviewShare_AuditLog table already exists, skipping creation.");
            return Task.CompletedTask;
        }

        Create.Table<uPreviewShareAuditLogEntry>().Do();
        Logger.LogInformation("Created uPreviewShare_AuditLog table.");
        return Task.CompletedTask;
    }
}
