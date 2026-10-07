using System.Collections.Concurrent;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly ConcurrentDictionary<Guid, ClientNotification> _notifications = new();

    public void Add(ClientNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!_notifications.TryAdd(notification.Id, notification))
            throw new InvalidOperationException("La notificación ya existe.");
    }

    public IEnumerable<ClientNotification> GetByClientId(Guid clientId)
    {
        return _notifications.Values
            .Where(notification => notification.ClientId == clientId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToList();
    }

    public ClientNotification? GetById(Guid id, Guid clientId)
    {
        return _notifications.TryGetValue(id, out var notification)
            && notification.ClientId == clientId
                ? notification
                : null;
    }

    public bool MarkAsRead(Guid id, Guid clientId)
    {
        var notification = GetById(id, clientId);
        if (notification is null)
            return false;

        notification.MarkAsRead();
        return true;
    }
}
