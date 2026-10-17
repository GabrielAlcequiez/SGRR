using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    // Categoría por defecto cuando el propietario no especifica una
    private const string DefaultCategory = "Otros";

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<RestaurantMenuViewModel> GetMenuAndSchedulesAsync(Guid restaurantId, DateTime selectedDate)
    {
        var model = new RestaurantMenuViewModel { RestaurantId = restaurantId };

        // 1. Consulta de platos y organización por categorías
        var items = await _menuRepository.GetByRestaurantIdAsync(restaurantId);
        if (items == null || !items.Any())
        {
            model.MenuErrorMessage = "Este restaurante no tiene platillos disponibles en su menú en este momento.";
        }
        else
        {
            model.CategorizedMenu = items
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // 2. Consulta y filtrado de horarios de atención
        var schedules = await _menuRepository.GetSchedulesByRestaurantIdAsync(restaurantId);
        var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == selectedDate.DayOfWeek);

        if (daySchedule == null)
        {
            model.ScheduleErrorMessage = "El restaurante se encuentra fuera de servicio en la fecha seleccionada.";
        }
        else
        {
            var current = daySchedule.OpeningTime;
            while (current < daySchedule.ClosingTime)
            {
                model.AvailableHours.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(30));
            }
        }

        return model;
    }

    // ===== Gestión del menú por parte del propietario =====

    public Task<IEnumerable<MenuItem>> GetByRestaurantIdForOwnerAsync(User requestor)
    {
        return _menuRepository.GetAllByRestaurantIdAsync(GetOwnerRestaurantId(requestor));
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, User requestor)
    {
        var restaurantId = GetOwnerRestaurantId(requestor);
        var item = await _menuRepository.GetByIdAsync(id);
        if (item is not null && item.RestaurantId != restaurantId)
            throw new UnauthorizedAccessException("Solo puedes administrar los platos de tu restaurante.");
        return item;
    }

    public async Task<MenuItem> CreateAsync(CreateDishDto dto, User requestor)
    {
        var restaurantId = GetOwnerRestaurantId(requestor);
        var name = ValidateItem(dto.Name, dto.Price);
        var category = ResolveCategory(dto.Category);
        await ValidateUniqueName(restaurantId, name);

        var item = new MenuItem(
            name,
            dto.Description ?? string.Empty,
            dto.Price,
            category,
            restaurantId,
            dto.ImageUrl ?? string.Empty);

        await _menuRepository.AddAsync(item);
        return item;
    }

    public async Task UpdateAsync(UpdateDishDto dto, User requestor)
    {
        var item = await GetByIdAsync(dto.Id, requestor)
            ?? throw new KeyNotFoundException("Plato no encontrado.");
        var name = ValidateItem(dto.Name, dto.Price);
        var category = ResolveCategory(dto.Category);
        await ValidateUniqueName(item.RestaurantId, name, item.Id);

        item.UpdateDetails(name, dto.Description ?? string.Empty, dto.Price, category, dto.ImageUrl ?? string.Empty);
        await _menuRepository.UpdateAsync(item);
    }

    public async Task DeleteAsync(Guid id, User requestor)
    {
        var item = await GetByIdAsync(id, requestor)
            ?? throw new KeyNotFoundException("Plato no encontrado.");
        await _menuRepository.DeleteAsync(item.Id);
    }

    private static Guid GetOwnerRestaurantId(User requestor)
    {
        if (requestor is null || requestor.Role != Roles.Propietary || !requestor.IsActive)
            throw new UnauthorizedAccessException("Se requiere una cuenta de propietario activa.");
        if (requestor.Restaurant is null)
            throw new UnauthorizedAccessException("Primero debes crear el perfil de tu restaurante.");
        return requestor.Restaurant.Id;
    }

    private static string ValidateItem(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.");
        if (price <= 0)
            throw new ArgumentException("El precio debe ser mayor que 0.");
        return name.Trim();
    }

    private static string ResolveCategory(string? category)
    {
        var trimmed = category?.Trim();
        return string.IsNullOrEmpty(trimmed) ? DefaultCategory : trimmed;
    }

    private async Task ValidateUniqueName(Guid restaurantId, string name, Guid? excludedId = null)
    {
        var items = await _menuRepository.GetAllByRestaurantIdAsync(restaurantId);
        if (items.Any(item => item.Id != excludedId &&
            item.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Ya existe un plato con ese nombre en este menú.");
    }
}
