using SGRR.Data.Common.Enums;

namespace SGRR.Data.Entities;

public class ClientNotification
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? ReferenceId { get; private set; }

    public ClientNotification(
        Guid clientId,
        NotificationType type,
        string title,
        string message,
        Guid? referenceId = null)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("El cliente es requerido.", nameof(clientId));

        if (!Enum.IsDefined(type))
            throw new ArgumentException("El tipo de notificación no es válido.", nameof(type));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título es requerido.", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("El mensaje es requerido.", nameof(message));

        Id = Guid.NewGuid();
        ClientId = clientId;
        Type = type;
        Title = title.Trim();
        Message = message.Trim();
        CreatedAt = DateTime.UtcNow;
        IsRead = false;
        ReferenceId = referenceId;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
