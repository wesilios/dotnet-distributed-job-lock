using System.Data.SqlClient;
using Dapper;
using Domain.Entities.JobLogs;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class JobLogDpRepository : BaseDapperRepository, IJobLogDpRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public JobLogDpRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<long> CreateJobLogAsync(JobLog jobLog)
    {
        await using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = $@"{InsertClause} INTO JobLogs (AppId, JobName, Remark)
            OUTPUT inserted.Id
            VALUES (@AppId, @JobName, @Remark);";

        try
        {
            return await connection.QueryFirstOrDefaultAsync<long>(sql, jobLog);
        }
        catch (SqlException)
        {
            await connection.CloseAsync();
            throw;
        }
    }

    public async Task<int> UpdateJobLogAsync(JobLog jobLog)
    {
        await using var connection = _sqlConnectionFactory.CreateConnection();

        var sql = $@"{UpdateClause} JobLogs
            SET Status = @Status, UpdatedTime = GETUTCDATE(), Remark = @Remark
            WHERE Id = @Id";

        try
        {
            return await connection.ExecuteAsync(sql, new
            {
                Id = jobLog.Id,
                Remark = jobLog.Remark,
                Status = jobLog.Status.ToString()
            });
        }
        catch (SqlException)
        {
            await connection.CloseAsync();
            throw;
        }
    }
}