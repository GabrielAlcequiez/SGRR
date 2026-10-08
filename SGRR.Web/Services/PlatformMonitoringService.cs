using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Monitoring;

namespace SGRR.Web.Services;

public class PlatformMonitoringService : IPlatformMonitoringService
{
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemActivityLogRepository _logRepository;

    public PlatformMonitoringService(
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository,
        IOrderRepository orderRepository,
        ISystemActivityLogRepository logRepository)
    {
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
        _orderRepository = orderRepository;
        _logRepository = logRepository;
    }

    public async Task<PlatformDashboardDto> GetSystemHealthAsync(User requestor)
    {
        if (requestor == null || requestor.Role != Roles.Admin)
        {
            throw new UnauthorizedAccessException("Acceso denegado. Se requieren permisos de Administrador del Sistema.");
        }

        var allUsers = _userRepository.GetAll().ToList();
        var allRestaurants = (await _restaurantRepository.GetAllAsync()).ToList();

        // Agrupaciones y cálculos
        var dto = new PlatformDashboardDto
        {
            TotalClients = allUsers.Count(u => u.Role == Roles.Client),
            TotalPropietaries = allUsers.Count(u => u.Role == Roles.Propietary),
            
            // Asumiendo que un restaurante hereda el estado de su usuario propietario o tiene su propio IsActive
            // Para este ejemplo, evaluamos si el dueño está activo (Ajustar según tu lógica de dominio).
            ActiveRestaurants = allUsers.Count(u => u.Role == Roles.Propietary && u.IsActive),
            InactiveRestaurants = allUsers.Count(u => u.Role == Roles.Propietary && !u.IsActive),
            
            DailyTransactions = await _orderRepository.GetDailyTransactionsCountAsync(),
            RecentLogs = _logRepository.GetLast100Logs()
        };

        return dto;
    }
}