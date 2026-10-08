using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Services;

public class DishService : IDishService
{
    private readonly IDishRepository _repository;

    public DishService(IDishRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Dish>> GetByRestaurantIdAsync(User requestor)
    {
        return _repository.GetByRestaurantIdAsync(GetOwnerRestaurantId(requestor));
    }

    public async Task<Dish?> GetByIdAsync(Guid id, User requestor)
    {
        var restaurantId = GetOwnerRestaurantId(requestor);
        var dish = await _repository.GetByIdAsync(id);
        if (dish is not null && dish.RestaurantId != restaurantId)
            throw new UnauthorizedAccessException("Solo puedes administrar los platos de tu restaurante.");
        return dish;
    }

    public async Task<Dish> CreateAsync(CreateDishDto dto, User requestor)
    {
        var restaurantId = GetOwnerRestaurantId(requestor);
        var name = ValidateDish(dto.Name, dto.Price);
        await ValidateUniqueName(restaurantId, name);

        var dish = new Dish
        {
            RestaurantId = restaurantId,
            Name = name,
            Price = dto.Price,
            Description = dto.Description?.Trim()
        };
        await _repository.AddAsync(dish);
        return dish;
    }

    public async Task UpdateAsync(UpdateDishDto dto, User requestor)
    {
        var dish = await GetByIdAsync(dto.Id, requestor)
            ?? throw new KeyNotFoundException("Plato no encontrado.");
        var name = ValidateDish(dto.Name, dto.Price);
        await ValidateUniqueName(dish.RestaurantId, name, dish.Id);

        dish.Name = name;
        dish.Price = dto.Price;
        dish.Description = dto.Description?.Trim();
        await _repository.UpdateAsync(dish);
    }

    public async Task DeleteAsync(Guid id, User requestor)
    {
        var dish = await GetByIdAsync(id, requestor)
            ?? throw new KeyNotFoundException("Plato no encontrado.");
        await _repository.DeleteAsync(dish.Id);
    }

    private static Guid GetOwnerRestaurantId(User requestor)
    {
        if (requestor is null || requestor.Role != Roles.Propietary || !requestor.IsActive)
            throw new UnauthorizedAccessException("Se requiere una cuenta de propietario activa.");
        if (requestor.Restaurant is null)
            throw new UnauthorizedAccessException("Primero debes crear el perfil de tu restaurante.");
        return requestor.Restaurant.Id;
    }

    private static string ValidateDish(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.");
        if (price <= 0)
            throw new ArgumentException("El precio debe ser mayor que 0.");
        return name.Trim();
    }

    private async Task ValidateUniqueName(Guid restaurantId, string name, Guid? excludedId = null)
    {
        var dishes = await _repository.GetByRestaurantIdAsync(restaurantId);
        if (dishes.Any(dish => dish.Id != excludedId &&
            dish.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Ya existe un plato con ese nombre en este menú.");
    }
}
