using SGRR.Data.Entities;
using SGRR.Web.Models;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Interfaces;

public interface IMenuService
{
    Task<RestaurantMenuViewModel> GetMenuAndSchedulesAsync(Guid restaurantId, DateTime selectedDate);

    // ===== Gestión del menú por parte del propietario (sobre la misma fuente de datos del cliente) =====

    // Todos los platillos del restaurante del propietario (incluye no disponibles)
    Task<IEnumerable<MenuItem>> GetByRestaurantIdForOwnerAsync(User requestor);

    // Devuelve el platillo si pertenece al restaurante del propietario; UnauthorizedAccessException si es ajeno
    Task<MenuItem?> GetByIdAsync(Guid id, User requestor);

    Task<MenuItem> CreateAsync(CreateDishDto dto, User requestor);

    Task UpdateAsync(UpdateDishDto dto, User requestor);

    Task DeleteAsync(Guid id, User requestor);
}
