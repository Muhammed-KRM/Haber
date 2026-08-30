using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;

namespace KursuTV.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Manager'lar (Servisler)
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IListingService, ListingManager>();
        services.AddScoped<ITokenService, TokenManager>();
        services.AddScoped<IMessageService, MessageManager>();
        services.AddScoped<IReviewService, ReviewManager>();
        services.AddScoped<IVitrinService, VitrinManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IAdminService, AdminManager>();
        services.AddScoped<ISettingService, SettingManager>();
        services.AddScoped<ILogService, LogManager>();
        services.AddMemoryCache();
        services.AddScoped<IEmailService, KursuTV.Business.Infrastructure.Email.SmtpEmailService>();
        services.AddScoped<IModerationService, ModerationManager>();

        // Bildirim Sistemi
        services.AddScoped<INotificationService, NotificationManager>();
        services.AddScoped<ISmsService, KursuTV.Business.Infrastructure.Sms.NetgsmSmsService>();
        services.AddScoped<KursuTV.Business.Infrastructure.Messaging.IFcmService, KursuTV.Business.Infrastructure.Messaging.FcmService>();
        services.AddHttpClient("Netgsm");
        services.AddHttpClient<KursuTV.Business.Infrastructure.Messaging.FcmService>();

        // FluentValidation â€” Bu assembly'deki tÃ¼m Validator'larÄ± otomatik tarayÄ±p kaydet
        services.AddValidatorsFromAssemblyContaining<AuthManager>();

        // AdÄ±m 3.2: Elasticsearch
        KursuTV.Business.Infrastructure.Search.ElasticsearchExtensions.AddElasticsearch(services);
        services.AddScoped<ISearchService, KursuTV.Business.Infrastructure.Search.ElasticsearchService>();

        // AdÄ±m 3.3: Redis
        services.AddSingleton<ICacheService, KursuTV.Business.Infrastructure.Cache.RedisCacheService>();

        // Ã–deme Sistemi â€” Strategy + Factory Pattern (PayTR yurt iÃ§i, Stripe yurt dÄ±ÅŸÄ±)
        services.AddScoped<IPaymentService, KursuTV.Business.Infrastructure.Payment.PayTRPaymentService>();
        services.AddScoped<IPaymentService, KursuTV.Business.Infrastructure.Payment.StripePaymentService>();
        services.AddScoped<IPaymentServiceFactory, KursuTV.Business.Infrastructure.Payment.PaymentServiceFactory>();

        // Dosya YÃ¼kleme (Local â†’ ileride Azure Blob'a geÃ§ilebilir)
        services.AddScoped<IFileStorageService, KursuTV.Business.Infrastructure.Storage.LocalFileStorageService>();

        // RabbitMQ/MassTransit â€” RabbitMQ disabled olduÄŸunda DummyPublishEndpoint kullanÄ±lÄ±r
        // RabbitMQ enabled olduÄŸunda Program.cs'deki AddMassTransit bu kaydÄ±n Ã¼zerine yazar
        services.AddScoped<MassTransit.IPublishEndpoint, KursuTV.Business.Infrastructure.Messaging.DummyPublishEndpoint>();
        
        return services;
    }
}
