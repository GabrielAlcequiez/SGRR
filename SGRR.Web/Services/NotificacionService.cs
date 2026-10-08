using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Notifications;

namespace SGRR.Web.Services;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _notificacionRepository;

    public NotificacionService(INotificacionRepository notificacionRepository)
    {
        _notificacionRepository = notificacionRepository;
    }

    public IEnumerable<NotificacionResponseDto> GetClientNotifications(User requestor)
    {
        if (requestor is null || requestor.Role != Roles.Client || !requestor.IsActive)
            throw new UnauthorizedAccessException("Acceso denegado. Se requiere una cuenta de cliente activa.");

        return _notificacionRepository.GetByClientId(requestor.Id)
            .Select(notification => new NotificacionResponseDto
            {
                Id = notification.Id,
                ClientId = notification.ClientId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                ReferenceId = notification.ReferenceId
            })
            .ToList();
    }
}
