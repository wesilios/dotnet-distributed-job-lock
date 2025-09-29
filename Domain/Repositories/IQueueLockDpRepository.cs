using Domain.Entities.QueueLocks;

namespace Domain.Repositories;

public interface IQueueLockDpRepository
{
    Task<QueueLock?> CreateQueueLockAsync(string queueName, string jobName);
    Task<QueueLock?> GetQueueLockAsync(string queueName, string jobName);
    Task<int> DeleteQueueLockAsync(QueueLock queueLock);
}