using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using uPreviewShare.Filters;
using uPreviewShare.Models;
using uPreviewShare.Notifications;
using uPreviewShare.Services;

namespace uPreviewShare.Composing;

/// <summary>
/// Entry point composer for the uPreviewShare package.
/// Registers all uPreviewShare services with the Umbraco DI container
/// and hooks into application startup to run migrations.
/// </summary>
public class uPreviewShareComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<uPreviewShareOptions>(builder.Config.GetSection(uPreviewShareOptions.SectionName));
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunuPreviewShareMigrations>();
        // Singleton lifetime is intentional: IScopeProvider in Umbraco is designed to be
        // injected into singletons and creates a new scope per operation.
        builder.Services.AddSingleton<IAuditLogService, AuditLogService>();
        builder.Services.AddSingleton<IBrandingService, BrandingService>();
        builder.Services.AddSingleton<IRateLimitService, RateLimitService>();
        builder.Services.AddSingleton<ITokenLinkService, TokenLinkService>();
        builder.Services.AddScoped<uPreviewShareExceptionFilter>();
        builder.Services.AddScoped<uPreviewSharePreviewBarFilter>();
        builder.Services.AddRecurringBackgroundJob<ExpiredLinkCleanupService>();
        builder.AddNotificationAsyncHandler<ContentDeletedNotification, uPreviewShareContentNotificationHandler>();
    }
}
