using System;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<IEnumerable<RestaurantSchedule>> GetSchedulesByRestaurantIdAsync(Guid restaurantId);
}
