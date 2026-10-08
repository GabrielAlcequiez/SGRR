using SGRR.Data.Entities;
using SGRR.Web.Models.Monitoring;

namespace SGRR.Web.Interfaces;

public interface IPlatformMonitoringService
{
    Task<PlatformDashboardDto> GetSystemHealthAsync(User requestor);
}