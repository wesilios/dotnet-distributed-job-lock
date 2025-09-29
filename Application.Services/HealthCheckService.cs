using Infrastructure;
using Microsoft.Extensions.Logging;
using TRECs.Library.Application;
using TRECs.Library.Domain;

namespace Application.Services;

public class HealthCheckService : PublicService, IHealthCheckService
{
    private readonly IUnitOfWork _unitOfWork;

    public HealthCheckService(ILogger<HealthCheckService> logger, IUnitOfWork unitOfWork) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PublicResponse<dynamic>> HealthCheckAsync()
    {
        var response = await _unitOfWork.HealthCheckAsync();
        return BuildSuccessResponse(response);
    }
}