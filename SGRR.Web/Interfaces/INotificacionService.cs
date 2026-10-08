using SGRR.Data.Entities;
using SGRR.Web.Models.Notifications;

namespace SGRR.Web.Interfaces;

public interface INotificacionService
{
    IEnumerable<NotificacionResponseDto> GetClientNotifications(User requestor);
}
