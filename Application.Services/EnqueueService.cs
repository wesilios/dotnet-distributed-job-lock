using Application.Services.JobServices;
using Coravel.Queuing.Interfaces;
using Domain;
using Hangfire;
using Microsoft.Extensions.Logging;
using TRECs.Library.Application;

namespace Application.Services;

public class EnqueueService : PublicService, IEnqueueService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IQueue _queue;

    public EnqueueService(ILogger<EnqueueService> logger, IBackgroundJobClient backgroundJobClient, IQueue queue) :
        base(logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _queue = queue;
    }

    public void EnqueueHangfireHeartbeatJob()
    {
        _backgroundJobClient.Enqueue<IHeartbeatHangFireJob>(x => x.InvokeAsync(QueueName.HangFireQueue, default));
    }

    public void EnqueueCoravelHeartbeatJob()
    {
        _queue.QueueCancellableInvocable<HeartbeatCoravelJob>();
    }
}