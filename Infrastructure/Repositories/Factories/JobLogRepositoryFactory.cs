using Domain.Repositories;
using Domain.Repositories.Factory;

namespace Infrastructure.Repositories.Factories;

public class JobLogRepositoryFactory : IJobLogRepositoryFactory
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly BackgroundJobDistributedLockDbContext _backgroundJobDistributedLockDbContext;
    private IJobLogDpRepository? _dapperRepository;
    private IJobLogRepository? _efRepository;

    public JobLogRepositoryFactory(ISqlConnectionFactory sqlConnectionFactory,
        BackgroundJobDistributedLockDbContext backgroundJobDistributedLockDbContext)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _backgroundJobDistributedLockDbContext = backgroundJobDistributedLockDbContext;
    }

    public IJobLogDpRepository DapperRepository()
    {
        return _dapperRepository ??= new JobLogDpRepository(_sqlConnectionFactory);
    }

    public IJobLogRepository EfRepository()
    {
        throw new NotImplementedException();
    }
}