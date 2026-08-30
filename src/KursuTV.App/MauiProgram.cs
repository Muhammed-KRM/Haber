using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using KursuTV.App.Interfaces;
using KursuTV.App.States;

namespace KursuTV.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Kimlik DoÄŸrulama Servisleri
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, MauiAuthenticationStateProvider>();
        builder.Services.AddScoped<ICustomAuthStateProvider>(sp =>
            (ICustomAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

        // HTTP Client ve Token YÃ¶netimi
        builder.Services.AddScoped<MauiAuthTokenHandler>();
        builder.Services.AddScoped(sp =>
        {
            var handler = sp.GetRequiredService<MauiAuthTokenHandler>();
            handler.InnerHandler = new HttpClientHandler();

            // Platform bazlÄ± API adresi
#if ANDROID
            var apiBase = "http://10.0.2.2:5001/"; // Android emulator â†’ host makine
#elif IOS
            var apiBase = "http://localhost:5001/"; // iOS simulator
#else
            var apiBase = "http://localhost:5001/"; // Windows / macOS
#endif
            return new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
