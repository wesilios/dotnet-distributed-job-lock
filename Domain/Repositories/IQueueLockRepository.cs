using Domain.Entities.QueueLocks;
using TRECs.Library.Domain;

namespace Domain.Repositories;

public interface IQueueLockRepository : IAsyncRepository<QueueLock>
{
}