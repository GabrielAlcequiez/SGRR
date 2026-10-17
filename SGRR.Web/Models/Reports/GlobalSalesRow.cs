namespace SGRR.Web.Models.Reports;

public class GlobalSalesRow
{
    public DateTime Date { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
