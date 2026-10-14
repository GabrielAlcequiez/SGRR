using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task<int> GetUnreadCountAsync(Guid restaurantId);
}