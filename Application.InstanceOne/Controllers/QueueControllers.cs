using Application.Services;
using Domain;
using Domain.Controllers.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Application.InstanceOne.Controllers;

public class QueueControllers : PublicApiBaseController<QueueControllers>
{
    private readonly IEnqueueService _enqueueService;
    private readonly AppSetting _appSetting;

    public QueueControllers(IEnqueueService enqueueService, IOptionsMonitor<AppSetting> appSettingOptions)
    {
        _enqueueService = enqueueService;
        _appSetting = appSettingOptions.CurrentValue;
    }

    [HttpPost("enqueue-hangfire-heartbeat-jb")]
    public IActionResult EnqueueHangfireHeartbeatJob()
    {
        _enqueueService.EnqueueHangfireHeartbeatJob();
        return Ok($"Instance '{_appSetting.AppId}' enqueued Hangfire job successfully.");
    }
    
    [HttpPost("enqueue-coravel-heartbeat-jb")]
    public IActionResult EnqueueCoravelHeartbeatJob()
    {
        _enqueueService.EnqueueCoravelHeartbeatJob();
        return Ok($"Instance '{_appSetting.AppId}' enqueued Hangfire job successfully.");
    }
}