using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface INotificacionRepository
{
    void Add(ClientNotification notification);
    IEnumerable<ClientNotification> GetByClientId(Guid clientId);
    ClientNotification? GetById(Guid id, Guid clientId);
    bool MarkAsRead(Guid id, Guid clientId);
}
