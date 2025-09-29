using TRECs.Library.Application;

namespace Application.Services;

public interface IEnqueueService : IPublicService
{
    void EnqueueHangfireHeartbeatJob();
    void EnqueueCoravelHeartbeatJob();
}