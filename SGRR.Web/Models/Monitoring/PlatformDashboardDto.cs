using SGRR.Data.Entities;

namespace SGRR.Web.Models.Monitoring;

public class PlatformDashboardDto
{
    public int TotalClients { get; set; }
    public int TotalPropietaries { get; set; }
    public int ActiveRestaurants { get; set; }
    public int InactiveRestaurants { get; set; }
    public int DailyTransactions { get; set; }
    public IEnumerable<SystemActivityLog> RecentLogs { get; set; } = new List<SystemActivityLog>();
}