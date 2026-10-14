namespace SGRR.Data.Models;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
