using System;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<IEnumerable<MenuItem>> GetAllByRestaurantIdAsync(Guid restaurantId);
    Task<IEnumerable<RestaurantSchedule>> GetSchedulesByRestaurantIdAsync(Guid restaurantId);
    Task<MenuItem?> GetByIdAsync(Guid id);
    Task AddAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(Guid id);
}
