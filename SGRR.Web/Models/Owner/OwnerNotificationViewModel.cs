namespace SGRR.Web.Models.Owner;

public class OwnerNotificationViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string CreatedAtFormatted { get; set; } = string.Empty;
    
    // Propiedades estéticas auxiliares para la vista
    public string TypeBadgeClass => Type == "Reservation" ? "primary" : "success";
    public string RowStatusClass => IsRead ? "" : "list-group-item-warning";
}