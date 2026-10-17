namespace SGRR.Web.Models.Reports;

public class RestaurantPerformanceRow
{
    public string RestaurantName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public int CompletedOrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageTicket { get; set; }

    public int ConfirmedReservationsCount { get; set; }
}
