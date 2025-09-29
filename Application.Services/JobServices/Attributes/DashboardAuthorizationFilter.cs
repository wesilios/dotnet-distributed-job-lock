using Hangfire.Dashboard;

namespace Application.Services.JobServices.Attributes
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public DashboardAuthorizationFilter()
        {

        }
        
        public bool Authorize(DashboardContext context)
        {
            return true;
        }
    }
}