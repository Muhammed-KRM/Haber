using Microsoft.Extensions.DependencyInjection;
using KursuTV.SharedUI.ApiServices;

namespace KursuTV.SharedUI;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedApiServices(this IServiceCollection services)
    {
        services.AddScoped<NewsApiService>();
        services.AddScoped<CategoryApiService>();
        services.AddScoped<CommentApiService>();
        services.AddScoped<AuthApiService>();
        services.AddScoped<UserApiService>();
        services.AddScoped<FinanceApiService>();
        
        return services;
    }
}
