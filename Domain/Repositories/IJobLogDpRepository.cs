using Domain.Entities.JobLogs;

namespace Domain.Repositories;

public interface IJobLogDpRepository
{
    Task<long> CreateJobLogAsync(JobLog jobLog);
    Task<int> UpdateJobLogAsync(JobLog jobLog);
}