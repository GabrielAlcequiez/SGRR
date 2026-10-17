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
        if (requestor == null || requestor.Role != Roles.Administrator)
        {
            throw new UnauthorizedAccessException("Acceso denegado. Se requieren permisos de Administrador del Sistema.");
        }

        var allUsers = _userRepository.GetAll().ToList();
        var allRestaurants = (await _restaurantRepository.GetAllAsync()).ToList();
        var activeRestaurantCount = allRestaurants.Count(restaurant =>
            _userRepository.GetOwnerByRestaurantId(restaurant.Id)?.IsActive == true);

        return new PlatformDashboardDto
        {
            TotalAdministrators = allUsers.Count(user => user.Role == Roles.Administrator),
            TotalClients = allUsers.Count(u => u.Role == Roles.Client),
            TotalProprietaries = allUsers.Count(user => user.Role == Roles.Propietary),
            ActiveRestaurants = activeRestaurantCount,
            InactiveRestaurants = allRestaurants.Count - activeRestaurantCount,
            DailyTransactions = await _orderRepository.GetDailyTransactionsCountAsync(),
            RecentLogs = _logRepository.GetLast100Logs()
        };
    }
}