using SGRR.Data.Common.Enums;

namespace SGRR.Web.Models.Owner;

public class OwnerDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public int CompletedOrdersCount { get; set; }
    public decimal AverageTicket { get; set; }
    public int ConfirmedReservationsCount { get; set; }
    public TimeRangeFilter CurrentFilter { get; set; } = TimeRangeFilter.AllTime;
    
    // Diccionario auxiliar para gráficos de distribución diaria básicos
    public Dictionary<string, decimal> DailyRevenue { get; set; } = new();
}