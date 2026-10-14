using SGRR.Data.Common.Enums;
using SGRR.Web.Models.Owner;

namespace SGRR.Web.Interfaces;

public interface IOwnerMetricsService
{
    Task<OwnerDashboardDto> GetDashboardMetricsAsync(Guid restaurantId, TimeRangeFilter filter);
}