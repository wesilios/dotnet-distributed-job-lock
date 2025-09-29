using Hangfire.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class HangfireHostedService : IHostedService
{
    private readonly ILogger<HangfireHostedService> _logger;
    private readonly IBackgroundProcessingServer _backgroundProcessingServer;

    public HangfireHostedService(ILogger<HangfireHostedService> logger, IBackgroundProcessingServer backgroundProcessingServer)
    {
        _logger = logger;
        _backgroundProcessingServer = backgroundProcessingServer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hangfire Hosted Service is starting.");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hangfire Hosted Service is stopping.");

        // Wait for Hangfire to complete any running jobs
        _backgroundProcessingServer.SendStop();
        await _backgroundProcessingServer.WaitForShutdownAsync(cancellationToken);

        _logger.LogInformation("Hangfire Hosted Service has stopped.");
    }
}