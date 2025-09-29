namespace Domain.Repositories.Factory;

public interface IQueueLockRepositoryFactory : IRepositoryFactory<IQueueLockRepository,
    IQueueLockDpRepository>
{
}