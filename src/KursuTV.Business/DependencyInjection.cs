using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;

namespace KursuTV.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Domain Servisleri (Haber Portalı)
        services.AddScoped<INewsService, NewsManager>();
        services.AddScoped<ICategoryService, CategoryManager>();
        services.AddScoped<ITagService, TagManager>();
        services.AddScoped<IMediaService, MediaManager>();
        services.AddScoped<ICommentService, CommentManager>();
        services.AddScoped<IDashboardService, DashboardManager>();

        // Kimlik & Kullanıcı Yönetimi
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IAdminService, AdminManager>();
        services.AddScoped<ISettingService, SettingManager>();
        services.AddScoped<ILogService, LogManager>();
        services.AddScoped<IModerationService, ModerationManager>();

        // İletişim ve E-Posta
        services.AddScoped<IEmailService, KursuTV.Business.Infrastructure.Email.SmtpEmailService>();
        services.AddScoped<INotificationService, NotificationManager>();
        services.AddScoped<KursuTV.Business.Infrastructure.Messaging.IFcmService, KursuTV.Business.Infrastructure.Messaging.FcmService>();
        services.AddHttpClient<KursuTV.Business.Infrastructure.Messaging.FcmService>();

        // Önbellekleme (In-Memory + Redis)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, KursuTV.Business.Infrastructure.Cache.RedisCacheService>();

        // FluentValidation (Tüm validator'ları otomatik tara ve kaydet)
        services.AddValidatorsFromAssemblyContaining<NewsManager>();

        // Arama Motoru (Elasticsearch)
        KursuTV.Business.Infrastructure.Search.ElasticsearchExtensions.AddElasticsearch(services);
        services.AddScoped<ISearchService, KursuTV.Business.Infrastructure.Search.ElasticsearchService>();

        // Dosya Depolama (Yerel / MinIO / S3)
        services.AddScoped<IFileStorageService, KursuTV.Business.Infrastructure.Storage.LocalFileStorageService>();

        // RabbitMQ / MassTransit Dummy Fallback
        services.AddScoped<MassTransit.IPublishEndpoint, KursuTV.Business.Infrastructure.Messaging.DummyPublishEndpoint>();

        return services;
    }
}
