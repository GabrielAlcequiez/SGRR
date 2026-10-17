using SGRR.Web.Models.Reports;

namespace SGRR.Web.Interfaces;

// Agrega los datos de ventas y reservas de toda la plataforma para los reportes del administrador
public interface IAdminReportService
{
    // Rendimiento por restaurante dentro del rango de fechas: pedidos completados,
    // ingresos, ticket promedio y reservas confirmadas
    Task<List<RestaurantPerformanceRow>> GetRestaurantPerformanceAsync(DateTime startDate, DateTime endDate);

    // Balance de ventas: una fila por restaurante por día con el total facturado en pedidos completados
    Task<List<GlobalSalesRow>> GetGlobalSalesAsync(DateTime startDate, DateTime endDate);
}
