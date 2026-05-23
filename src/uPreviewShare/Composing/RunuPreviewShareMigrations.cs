using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using uPreviewShare.Migrations;

namespace uPreviewShare.Composing;

/// <summary>
/// Notification handler that executes the uPreviewShare migration plan
/// when the Umbraco application has started.
/// </summary>
public class RunuPreviewShareMigrations : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly IKeyValueService _keyValueService;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<RunuPreviewShareMigrations> _logger;

    public RunuPreviewShareMigrations(
        IMigrationPlanExecutor migrationPlanExecutor,
        ICoreScopeProvider coreScopeProvider,
        IKeyValueService keyValueService,
        IRuntimeState runtimeState,
        ILogger<RunuPreviewShareMigrations> logger)
    {
        _migrationPlanExecutor = migrationPlanExecutor;
        _coreScopeProvider = coreScopeProvider;
        _keyValueService = keyValueService;
        _runtimeState = runtimeState;
        _logger = logger;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level < RuntimeLevel.Run)
            return;

        var migrationPlan = new uPreviewShareMigrationPlan();
        var upgrader = new Upgrader(migrationPlan);
        await upgrader.ExecuteAsync(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);
        _logger.LogInformation("uPreviewShare migration plan executed successfully.");
    }
}
