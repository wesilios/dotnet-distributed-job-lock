using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<BackgroundJobDistributedLockDbContext>(options =>
            options.UseSqlServer(connectionString, option =>
                option.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null)));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>(_ =>
            new SqlConnectionFactory(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}