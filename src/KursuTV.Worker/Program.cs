using MassTransit;
using KursuTV.Worker.Consumers;
using KursuTV.Worker.Jobs;
using KursuTV.Business;
using KursuTV.Data;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=kursutv;Username=kursutv_user;Password=dev_password";

// Veri ve İş Katmanı DI Kayıtları
KursuTV.Data.ServiceRegistration.AddDataLayer(builder.Services, connectionString);
KursuTV.Business.DependencyInjection.AddBusinessServices(builder.Services);

// Arka Plan İş Servisleri (Hangfire yerine BackgroundService)
builder.Services.AddHostedService<ScheduledNewsPublishJob>();
builder.Services.AddHostedService<SitemapGeneratorJob>();

// MassTransit v8 – RabbitMQ (Haber Event Consumer'ları)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NewsPublishedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var mqHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var mqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var mqPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(mqHost, "/", h =>
        {
            h.Username(mqUser);
            h.Password(mqPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
