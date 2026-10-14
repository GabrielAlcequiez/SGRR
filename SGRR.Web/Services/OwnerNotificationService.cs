using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class OwnerNotificationService : IOwnerNotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public OwnerNotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<IEnumerable<Notification>> GetOwnerNotificationsAsync(Guid restaurantId)
    {
        return await _notificationRepository.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<int> GetUnreadCountAsync(Guid restaurantId)
    {
        return await _notificationRepository.GetUnreadCountAsync(restaurantId);
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null && !notification.IsRead)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task NotifyNewReservationAsync(Guid restaurantId, Guid userId, string clientName, string date, string timeSlot)
    {
        var notification = new Notification
        {
            RestaurantId = restaurantId,
            UserId = userId,
            Title = "Nueva Reserva Registrada",
            Message = $"El cliente {clientName} ha realizado una reserva para el {date} a las {timeSlot}.",
            Type = "Reservation",
            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task NotifyOrderChangeAsync(Guid restaurantId, Guid userId, string orderSummary, string status)
    {
        var notification = new Notification
        {
            RestaurantId = restaurantId,
            UserId = userId,
            Title = "Actualización de Orden",
            Message = $"La orden ({orderSummary}) ha cambiado de estado a: {status}.",
            Type = "Order",
            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);
    }
}