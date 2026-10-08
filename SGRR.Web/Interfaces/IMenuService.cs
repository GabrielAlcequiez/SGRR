using SGRR.Web.Models;

namespace SGRR.Web.Interfaces;

public interface IMenuService
{
    Task<RestaurantMenuViewModel> GetMenuAndSchedulesAsync(Guid restaurantId, DateTime selectedDate);
}
