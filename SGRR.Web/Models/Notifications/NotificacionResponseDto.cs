using SGRR.Data.Common.Enums;

namespace SGRR.Web.Models.Notifications;

public class NotificacionResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public Guid? ReferenceId { get; set; }
}
