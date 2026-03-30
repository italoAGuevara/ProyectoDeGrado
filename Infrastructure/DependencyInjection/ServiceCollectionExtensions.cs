using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        string connectionString = "Data Source=mibase.db")
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        return services;
    }
}
