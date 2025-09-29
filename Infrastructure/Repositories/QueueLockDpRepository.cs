using Dapper;
using Domain.Entities.QueueLocks;
using Domain.Repositories;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories;

public class QueueLockDpRepository : BaseDapperRepository, IQueueLockDpRepository
{
    private readonly ISqlConnectionFactory _dataServiceSqlConnectionFactory;

    public QueueLockDpRepository(ISqlConnectionFactory dataServiceSqlConnectionFactory)
    {
        _dataServiceSqlConnectionFactory = dataServiceSqlConnectionFactory;
    }

    public async Task<QueueLock?> CreateQueueLockAsync(string queueName, string jobName)
    {
        await using var connection = _dataServiceSqlConnectionFactory.CreateConnection();

        var sql = $@"{InsertClause} INTO QueueLocks (QueueName, JobName)
                OUTPUT inserted.QueueName,
                       inserted.JobName
                VALUES (@QueueName, @JobName);";

        try
        {
            return await connection.QueryFirstOrDefaultAsync<QueueLock>(sql, new
            {
                QueueName = queueName,
                JobName = jobName
            });
        }
        catch (SqlException e)
        {
            await connection.CloseAsync();
            // 2601 - Violation in unique index
            // 2627 - Violation in unique constraint (although it is implemented using unique index)
            if (e.Number is 2601 or 2627)
            {
                return null;
            }

            throw;
        }
    }

    public async Task<QueueLock?> GetQueueLockAsync(string queueName, string jobName)
    {
        await using var connection = _dataServiceSqlConnectionFactory.CreateConnection();

        var sql = $@"{SelectClause} * 
                FROM QueueLocks
                WHERE QueueName = @QueueName AND JobName = @JobName;";

        try
        {
            return await connection.QueryFirstOrDefaultAsync<QueueLock>(sql, new
            {
                QueueName = queueName,
                JobName = jobName
            });
        }
        catch (SqlException)
        {
            await connection.CloseAsync();
            throw;
        }
    }

    public async Task<int> DeleteQueueLockAsync(QueueLock queueLock)
    {
        await using var connection = _dataServiceSqlConnectionFactory.CreateConnection();

        var sql = $@"{DeleteClause} QueueLocks
                WHERE QueueName = @QueueName AND JobName = @JobName;";

        try
        {
            return await connection.ExecuteAsync(sql, new
            {
                QueueName = queueLock.QueueName,
                JobName = queueLock.JobName
            });
        }
        catch (SqlException)
        {
            await connection.CloseAsync();
            throw;
        }
    }
}