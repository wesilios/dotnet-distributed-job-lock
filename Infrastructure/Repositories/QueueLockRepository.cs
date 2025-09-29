using Domain.Entities.QueueLocks;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using TRECs.Library.Infrastructure;

namespace Infrastructure.Repositories;

public class QueueLockRepository : AsyncRepository<QueueLock>, IQueueLockRepository
{
    public QueueLockRepository(DbContext dbContext) : base(dbContext)
    {
    }
}