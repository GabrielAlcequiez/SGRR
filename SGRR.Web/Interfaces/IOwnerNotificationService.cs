using SGRR.Web.Models.Owner;

namespace SGRR.Web.Interfaces;

public interface IOwnerNotificationService
{
    Task<IEnumerable<OwnerNotificationViewModel>> GetOwnerNotificationsAsync(Guid restaurantId);
    Task<int> GetUnreadCountAsync(Guid restaurantId);
    Task MarkAsReadAsync(Guid notificationId);
    Task NotifyNewReservationAsync(Guid restaurantId, Guid userId, string clientName, string date, string timeSlot);
    Task NotifyOrderChangeAsync(Guid restaurantId, Guid userId, string orderSummary, string status);
}