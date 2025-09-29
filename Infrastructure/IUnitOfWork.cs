using Domain.Repositories.Factory;

namespace Infrastructure;

public interface IUnitOfWork : IDisposable
{
    Task<object> HealthCheckAsync();
    
    IQueueLockRepositoryFactory QueueLockRepositories { get; }
    IJobLogRepositoryFactory JobLogRepositories { get; }
}