namespace SGRR.Data.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RestaurantId { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Completed"; // Pending, Completed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string ProductDetails { get; set; } = string.Empty;
}