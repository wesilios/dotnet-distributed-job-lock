using Dapper;
using Domain.Repositories.Factory;
using Infrastructure.Repositories.Factories;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly BackgroundJobDistributedLockDbContext _backgroundJobDistributedLockDbContext;
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    private QueueLockRepositoryFactory _queueLockRepositoryFactory;
    private JobLogRepositoryFactory _jobLogRepositoryFactory;


    public UnitOfWork(BackgroundJobDistributedLockDbContext backgroundJobDistributedLockDbContext,
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _backgroundJobDistributedLockDbContext = backgroundJobDistributedLockDbContext;
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public IQueueLockRepositoryFactory QueueLockRepositories =>
        _queueLockRepositoryFactory ??=
            new QueueLockRepositoryFactory(_sqlConnectionFactory, _backgroundJobDistributedLockDbContext);

    public IJobLogRepositoryFactory JobLogRepositories => _jobLogRepositoryFactory ??=
        new JobLogRepositoryFactory(_sqlConnectionFactory, _backgroundJobDistributedLockDbContext);

    public void Dispose()
    {
        _backgroundJobDistributedLockDbContext.Dispose();
    }

    public async Task<object> HealthCheckAsync()
    {
        await using var connection = _sqlConnectionFactory.CreateConnection();
        var result = await connection.QueryAsync(@"SELECT 1");
        return result;
    }
}