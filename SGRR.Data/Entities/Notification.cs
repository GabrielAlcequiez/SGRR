namespace SGRR.Data.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; } // Propietario o receptor
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "General"; // Reservation, Order, Cancellation
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public void MarkAsRead()
    {
        IsRead = true;
    }
}