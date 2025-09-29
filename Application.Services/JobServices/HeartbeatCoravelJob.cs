using System.Data;
using Coravel.Invocable;
using Coravel.Queuing.Interfaces;
using Domain;
using Domain.Entities.JobLogs;
using Domain.Entities.QueueLocks;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services.JobServices;

public class HeartbeatCoravelJob : IInvocable, ICancellableTask
{
    private readonly AppSetting _appSetting;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HeartbeatCoravelJob> _logger;

    private const int QueueLockMaximumLifeTimeInMinutes = 5;
    private readonly string _jobName;

    public HeartbeatCoravelJob(IOptionsMonitor<AppSetting> appSettingOption, IUnitOfWork unitOfWork,
        ILogger<HeartbeatCoravelJob> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _appSetting = appSettingOption.CurrentValue;
        _jobName = nameof(HeartbeatCoravelJob);
    }

    public async Task Invoke()
    {
        var jobLog = new JobLog
        {
            AppId = _appSetting.AppId,
            JobName = _jobName,
            Remark = string.Empty,
        };
        jobLog.Id = await _unitOfWork.JobLogRepositories.DapperRepository().CreateJobLogAsync(jobLog);
        while (!Token.IsCancellationRequested)
        {
            QueueLock? queueLock;
            try
            {
                queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                    .CreateQueueLockAsync(QueueName.CoravelQueue, _jobName);
                if (queueLock is null)
                {
                    queueLock = await _unitOfWork.QueueLockRepositories.DapperRepository()
                        .GetQueueLockAsync(QueueName.CoravelQueue, _jobName);
                    jobLog.Status = JobLogStatus.Exited;
                    if (queueLock is null)
                    {
                        jobLog.Remark = string.Format("Queue Name {0} with Job {1} has been released.",
                            QueueName.CoravelQueue, _jobName);
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
                        _appSetting.AppId, QueueName.CoravelQueue, _jobName);
                    await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                    return;
                }
            }
            catch (DBConcurrencyException)
            {
                // Catch DB Concurrency Exception
                jobLog.Remark = string.Format(
                    "===> DBConcurrencyException reason: Instance {0} Queue Name {1} with Job {2} is already processing.",
                    _appSetting.AppId, QueueName.CoravelQueue, _jobName);
                await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                return;
            }

            try
            {
                await ProcessQueueAsync();
            }
            catch
            {
                await _unitOfWork.QueueLockRepositories.DapperRepository()
                    .DeleteQueueLockAsync(queueLock);

                jobLog.Remark = string.Format(
                    "===> Instance {0} Deleting Queue Name {1} with Job {2} by graceful shutdown.",
                    _appSetting.AppId, QueueName.CoravelQueue, _jobName);
                jobLog.Status = JobLogStatus.Exited;
                await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
                throw;
            }

            await _unitOfWork.QueueLockRepositories.DapperRepository()
                .DeleteQueueLockAsync(queueLock);
            jobLog.Remark = "Success";
            jobLog.Status = JobLogStatus.Completed;
            await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
            return;
        }

        jobLog.Remark = string.Format(
            "===> Instance {0} Deleting Queue Name {1} with Job {2} by graceful shutdown.",
            _appSetting.AppId, QueueName.CoravelQueue, _jobName);
        jobLog.Status = JobLogStatus.Exited;
        await _unitOfWork.JobLogRepositories.DapperRepository().UpdateJobLogAsync(jobLog);
    }

    private async Task ProcessQueueAsync()
    {
        // count to 120 in 120 secs
        for (var i = 0; i < 120; i++)
        {
            _logger.LogInformation(
                "===> Instance {AppId} on {QueueName} with Job {JobName} is counting: {Number}",
                _appSetting.AppId, QueueName.CoravelQueue, _jobName, i);
            await Task.Delay(1000);
        }
    }

    public CancellationToken Token { get; set; }
}