using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using KursuTV.Data.Context;
using KursuTV.Data.Repositories;

namespace KursuTV.Data;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
    {
        // Add DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        // Domain Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        return services;
    }
}
