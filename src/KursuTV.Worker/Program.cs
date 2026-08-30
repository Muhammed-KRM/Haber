using MassTransit;
using KursuTV.Worker.Consumers;
using KursuTV.Business;
using KursuTV.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=kursutv;Username=kursutv_user;Password=dev_password";

KursuTV.Data.ServiceRegistration.AddDataLayer(builder.Services, connectionString);
KursuTV.Business.DependencyInjection.AddBusinessServices(builder.Services);

builder.Services.AddSingleton<KursuTV.Worker.Services.OllamaService>();

// Firebase baÅŸlatma
var firebaseCredPath = builder.Configuration["Firebase:CredentialPath"];
if (!string.IsNullOrEmpty(firebaseCredPath) && File.Exists(firebaseCredPath))
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseCredPath)
    });
}

// MassTransit v8 â€” RabbitMQ (Ã¼cretsiz, lisans gerektirmez)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ListingCreatedConsumer>();
    x.AddConsumer<ListingUpdatedConsumer>();
    x.AddConsumer<ListingDeletedConsumer>();
    x.AddConsumer<SendNotificationConsumer>();

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

builder.Services.AddHostedService<KursuTV.Worker.Services.VitrinExpirationWorker>();
builder.Services.AddHostedService<KursuTV.Worker.Services.NotificationCleanupWorker>();

var host = builder.Build();
host.Run();
