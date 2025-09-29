using Domain.Repositories;
using Domain.Repositories.Factory;

namespace Infrastructure.Repositories.Factories;

public class QueueLockRepositoryFactory : IQueueLockRepositoryFactory
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly BackgroundJobDistributedLockDbContext _backgroundJobDistributedLockDbContext;
    private IQueueLockDpRepository? _dapperRepository;
    private IQueueLockRepository? _efRepository;

    public QueueLockRepositoryFactory(ISqlConnectionFactory sqlConnectionFactory,
        BackgroundJobDistributedLockDbContext backgroundJobDistributedLockDbContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _backgroundJobDistributedLockDbContext = backgroundJobDistributedLockDbContext;
    }

    public IQueueLockDpRepository DapperRepository()
    {
        return _dapperRepository ??= new QueueLockDpRepository(_sqlConnectionFactory);
    }

    public IQueueLockRepository EfRepository()
    {
        return _efRepository ??= new QueueLockRepository(_backgroundJobDistributedLockDbContext);
    }
}