using System.Text.Json.Serialization;
using Application.Services.JobServices;
using Application.Services.JobServices.Attributes;
using Coravel;
using Domain;
using Hangfire;
using Hangfire.Server;
using Hangfire.SqlServer;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application.Services;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

        services.AddOptions<AppSetting>();

        services.AddInfrastructure(configuration.GetConnectionString("DbConnection")!);

        services.AddScheduler();

        services.AddQueue();

        services.AddHangfireService(configuration);

        services.AddScoped<IHeartbeatHangFireJob, HeartbeatHangFireJob>();
        services.AddScoped<IHealthCheckService, HealthCheckService>();
        services.AddScoped<IEnqueueService, EnqueueService>();
        services.AddTransient<HeartbeatCoravelJob>();
    }

    private static void AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(x => x
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseFilter(new LogFailureAttribute())
            .UseSqlServerStorage(configuration.GetConnectionString("DbConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer(options =>
        {
            options.Queues = [QueueName.HangFireQueue];
            options.WorkerCount = 1;
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = [QueueName.CoravelQueue];
            options.WorkerCount = 1;
        });

        services.AddSingleton<IBackgroundProcessingServer>(_ => new BackgroundJobServer());
        services.AddHostedService<HangfireHostedService>();

        services.Configure<HostOptions>(configuration.GetSection("HostOptions"));
    }
}