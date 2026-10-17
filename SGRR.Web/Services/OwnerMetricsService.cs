using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Owner;

namespace SGRR.Web.Services;

public class OwnerMetricsService : IOwnerMetricsService
{
    private readonly AppDbContext _context;

    public OwnerMetricsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OwnerDashboardDto> GetDashboardMetricsAsync(Guid restaurantId, TimeRangeFilter filter)
    {
        var now = DateTime.Today;

        // 1. Consultar y filtrar órdenes del restaurante
        var ordersQuery = _context.Orders
            .Where(o => o.RestaurantId == restaurantId);

        // Aplicar filtros de tiempo
        ordersQuery = filter switch
        {
            TimeRangeFilter.Today => ordersQuery.Where(o => o.CreatedAt.Date == now),
            TimeRangeFilter.Last7Days => ordersQuery.Where(o => o.CreatedAt.Date >= now.AddDays(-7)),
            _ => ordersQuery
        };

        var orders = await ordersQuery.ToListAsync();

        // Órdenes completadas (las rechazadas o canceladas no suman a ingresos ni ticket promedio)
        var completedOrders = orders.Where(o => o.Status == "Completed" || o.Status == "Completada").ToList();
        
        decimal totalRevenue = completedOrders.Sum(o => o.TotalAmount);
        int completedCount = completedOrders.Count;
        
        // Operación protegida para evitar división por cero
        decimal averageTicket = completedCount > 0 ? totalRevenue / completedCount : 0.00m;

        // 2. Consultar y filtrar reservas confirmadas
        var reservationsQuery = _context.Reservations
            .Where(r => r.RestaurantId == restaurantId && (r.Status == "Confirmed" || r.Status == "Confirmada" || r.Status == "Pending"));

        reservationsQuery = filter switch
        {
            TimeRangeFilter.Today => reservationsQuery.Where(r => r.Date.Date == now),
            TimeRangeFilter.Last7Days => reservationsQuery.Where(r => r.Date.Date >= now.AddDays(-7)),
            _ => reservationsQuery
        };

        int confirmedReservations = await reservationsQuery.CountAsync();

        // 3. Generar distribución diaria para gráficos básicos
        var dailyRevenue = completedOrders
            .GroupBy(o => o.CreatedAt.ToString("dd/MM"))
            .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

        return new OwnerDashboardDto
        {
            TotalRevenue = totalRevenue,
            CompletedOrdersCount = completedCount,
            AverageTicket = averageTicket,
            ConfirmedReservationsCount = confirmedReservations,
            CurrentFilter = filter,
            DailyRevenue = dailyRevenue
        };
    }
}