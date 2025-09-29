using Domain.Entities.JobLogs;
using TRECs.Library.Domain;

namespace Domain.Repositories;

public interface IJobLogRepository : IAsyncRepository<JobLog>
{
    
}