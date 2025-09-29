using System.Data;
using Application.Services.JobServices.Abstractions;
using Domain;
using Domain.Entities.JobLogs;
using Domain.Entities.QueueLocks;
using Hangfire;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services.JobServices;

public interface IHeartbeatHangFireJob : IBackgroundJob
{
    [Queue("{0}")]
    Task InvokeAsync(string queueName, CancellationToken cancellationToken);
}

public class HeartbeatHangFireJob : IHeartbeatHangFireJob
{
    private readonly AppSetting _appSetting;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HeartbeatHangFireJob> _logger;

    private const int QueueLockMaximumLifeTimeInMinutes = 5;
    private readonly string _jobName;

    public HeartbeatHangFireJob(IOptionsMonitor<AppSetting> appSettingOption, IUnitOfWork unitOfWork,
        ILogger<HeartbeatHangFireJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _appSetting = appSettingOption.CurrentValue;
        _jobName = nameof(HeartbeatHangFireJob);
    }

    [DisableConcurrentExecution(timeoutInSeconds: 30)]
    public async Task InvokeAsync(CancellationToken cancellationToken)
    {
        var jobLog = new JobLog
        {
            AppId = _appSetting.AppId,
            JobName = _jobName,
            Remark = string.Empty,
        };
        jobLog.Id = await _unitOfWork.JobLogRepositories.DapperRepository().CreateJobLogAsync(jobLog);

        QueueLock? queueLock;
        try
        {
            queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                .CreateQueueLockAsync(QueueName.HangFireQueue, _jobName);
            if (queueLock is null)
            {
                queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                    .GetQueueLockAsync(QueueName.HangFireQueue, _jobName);
                jobLog.Status = JobLogStatus.Exited;
                if (queueLock is null)
                {
                    jobLog.Remark = string.Format("Queue Name {0} with Job {1} has been released.",
                        QueueName.HangFireQueue, _jobName);
                    await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                    return;
                }

                var timespan = DateTime.UtcNow - queueLock.CreatedTime;
                if (timespan.Minutes >= QueueLockMaximumLifeTimeInMinutes)
                {
                    jobLog.Remark =
                        $"Exited and remove lock due to the job has been locked for more than {QueueLockMaximumLifeTimeInMinutes} minutes; Deleting the lock and retry to process...";
                    await _unitOfWork.QueueLockRepositories.DapperRepository()
                        .DeleteQueueLockAsync(queueLock);
                    await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                    return;
                }

                jobLog.Remark = string.Format(
                    "===> Violation in unique constraint: Instance {0} Queue Name {1} with Job {2} is already processing.",
                    _appSetting.AppId, QueueName.HangFireQueue, _jobName);
                await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                return;
            }
        }
        catch (DBConcurrencyException)
        {
            // Catch DB Concurrency Exception
            jobLog.Remark = string.Format(
                "===> DBConcurrencyException reason: Instance {0} Queue Name {1} with Job {2} is already processing.",
                _appSetting.AppId, QueueName.HangFireQueue, _jobName);
            await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
            return;
        }

        try
        {
            await ProcessQueueAsync(QueueName.HangFireQueue, cancellationToken);
        }
        catch
        {
            await _unitOfWork.QueueLockRepositories.DapperRepository()
                .DeleteQueueLockAsync(queueLock);

            jobLog.Remark = string.Format(
                "===> Instance {0} Deleting Queue Name {1} with Job {2} by graceful shutdown.",
                _appSetting.AppId, QueueName.HangFireQueue, _jobName);
            jobLog.Status = JobLogStatus.Exited;
            await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
            throw;
        }

        await _unitOfWork.QueueLockRepositories.DapperRepository()
            .DeleteQueueLockAsync(queueLock);
        jobLog.Remark = "Success";
        jobLog.Status = JobLogStatus.Completed;
        await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
    }

    public async Task InvokeAsync(string queueName, CancellationToken cancellationToken)
    {
        var jobLog = new JobLog
        {
            AppId = _appSetting.AppId,
            JobName = _jobName,
            Remark = string.Empty,
        };
        jobLog.Id = await _unitOfWork.JobLogRepositories.DapperRepository().CreateJobLogAsync(jobLog);

        QueueLock? queueLock;
        try
        {
            queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                .CreateQueueLockAsync(queueName, _jobName);
            if (queueLock is null)
            {
                queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                    .GetQueueLockAsync(queueName, _jobName);
                jobLog.Status = JobLogStatus.Exited;
                if (queueLock is null)
                {
                    jobLog.Remark = string.Format("Queue Name {0} with Job {1} has been released.",
                        queueName, _jobName);
                    await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                    return;
                }

                var timespan = DateTime.UtcNow - queueLock.CreatedTime;
                if (timespan.Minutes >= QueueLockMaximumLifeTimeInMinutes)
                {
                    jobLog.Remark =
                        $"Exited and remove lock due to the job has been locked for more than {QueueLockMaximumLifeTimeInMinutes} minutes; Deleting the lock and retry to process...";
                    await _unitOfWork.QueueLockRepositories.DapperRepository()
                        .DeleteQueueLockAsync(queueLock);
                    await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                    return;
                }

                jobLog.Remark = string.Format(
                    "===> Violation in unique constraint: Instance {0} Queue Name {1} with Job {2} is already processing.",
                    _appSetting.AppId, queueName, _jobName);
                await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                return;
            }
        }
        catch (DBConcurrencyException)
        {
            // Catch DB Concurrency Exception
            jobLog.Remark = string.Format(
                "===> DBConcurrencyException reason: Instance {0} Queue Name {1} with Job {2} is already processing.",
                _appSetting.AppId, queueName, _jobName);
            await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
            return;
        }

        try
        {
            await ProcessQueueAsync(queueName, cancellationToken);
        }
        catch
        {
            await _unitOfWork.QueueLockRepositories.DapperRepository()
                .DeleteQueueLockAsync(queueLock);

            jobLog.Remark = string.Format(
                "===> Instance {0} Deleting Queue Name {1} with Job {2} by graceful shutdown.",
                _appSetting.AppId, queueName, _jobName);
            jobLog.Status = JobLogStatus.Exited;
            await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
            throw;
        }

        await _unitOfWork.QueueLockRepositories.DapperRepository()
            .DeleteQueueLockAsync(queueLock);
        jobLog.Remark = "Success";
        jobLog.Status = JobLogStatus.Completed;
        await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
    }

    private async Task ProcessQueueAsync(string queueName, CancellationToken cancellationToken)
    {
        // count to 120 in 120 secs
        for (var i = 0; i < 120; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The Job has been cancelled due to deployment.");
                return;
            }

            _logger.LogInformation(
                "===> Instance {AppId} on {QueueName} with Job {JobName} is counting: {Number}",
                _appSetting.AppId, queueName, _jobName, i);
            await Task.Delay(1000, cancellationToken);
        }
    }
}