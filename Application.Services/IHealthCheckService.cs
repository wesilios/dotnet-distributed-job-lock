using TRECs.Library.Application;
using TRECs.Library.Domain;

namespace Application.Services;

public interface IHealthCheckService : IPublicService
{
    Task<PublicResponse<dynamic>> HealthCheckAsync();
}