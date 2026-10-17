using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Reports;

namespace SGRR.Web.Services;

// Agrega en memoria los datos de pedidos y reservas de toda la plataforma para los reportes admin
public class AdminReportService : IAdminReportService
{
    private readonly AppDbContext _context;

    // Mismo criterio bilingüe de estados que usa OwnerMetricsService
    private const string CompletedStatusEn = "Completed";
    private const string CompletedStatusEs = "Completada";

    public AdminReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RestaurantPerformanceRow>> GetRestaurantPerformanceAsync(DateTime startDate, DateTime endDate)
    {
        // Normalizamos ambas puntas al día completo
        var start = startDate.Date;
        var end = endDate.Date;

        var restaurants = await _context.Restaurants.AsNoTracking().ToListAsync();

        var completedOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt.Date >= start
                     && o.CreatedAt.Date <= end
                     && (o.Status == CompletedStatusEn || o.Status == CompletedStatusEs))
            .ToListAsync();

        var activeReservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Date.Date >= start
                     && r.Date.Date <= end
                     && (r.Status == "Active" || r.Status == "Activa"
                      || r.Status == "Confirmed" || r.Status == "Confirmada"))
            .ToListAsync();

        var rows = restaurants.Select(restaurant =>
        {
            var restaurantOrders = completedOrders.Where(o => o.RestaurantId == restaurant.Id).ToList();
            decimal totalRevenue = restaurantOrders.Sum(o => o.TotalAmount);
            int completedCount = restaurantOrders.Count;

            return new RestaurantPerformanceRow
            {
                RestaurantName = restaurant.Name,
                Specialty = restaurant.Specialty,
                Location = restaurant.Location,
                Rating = restaurant.Rating,
                ReviewCount = restaurant.ReviewCount,
                CompletedOrdersCount = completedCount,
                TotalRevenue = totalRevenue,
                AverageTicket = completedCount > 0 ? totalRevenue / completedCount : 0.00m,
                ConfirmedReservationsCount = activeReservations.Count(r => r.RestaurantId == restaurant.Id)
            };
        })
        .OrderByDescending(row => row.TotalRevenue)
        .ToList();

        return rows;
    }

    public async Task<List<GlobalSalesRow>> GetGlobalSalesAsync(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        var completedOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt.Date >= start
                     && o.CreatedAt.Date <= end
                     && (o.Status == CompletedStatusEn || o.Status == CompletedStatusEs))
            .ToListAsync();

        var restaurantNames = await _context.Restaurants
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name);

        var rows = completedOrders
            .GroupBy(o => new { RestaurantId = o.RestaurantId, Day = o.CreatedAt.Date })
            .Select(group => new GlobalSalesRow
            {
                Date = group.Key.Day,
                RestaurantName = restaurantNames.TryGetValue(group.Key.RestaurantId, out var name)
                    ? name
                    : "Restaurante eliminado",
                OrdersCount = group.Count(),
                TotalRevenue = group.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(row => row.Date)
            .ThenBy(row => row.RestaurantName)
            .ToList();

        return rows;
    }
}
