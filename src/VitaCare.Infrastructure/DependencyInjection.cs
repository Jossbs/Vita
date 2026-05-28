using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VitaCare.Infrastructure.Persistence;

namespace VitaCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        string? migrationsAssembly = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                {
                    npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                }
            }));

        return services;
    }
}
