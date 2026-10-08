using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Services;

public class RestaurantService : IRestaurantService
{
    private readonly IRestaurantRepository _repository;
    private readonly IUserRepository _userRepository;

    public RestaurantService(IRestaurantRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public Task<IEnumerable<Restaurant>> GetAllAsync() => _repository.GetAllAsync();

    public async Task<IEnumerable<Restaurant>> SearchAsync(string? search)
    {
        var restaurants = await _repository.GetAllAsync();
        if (string.IsNullOrWhiteSpace(search))
            return restaurants;

        var term = search.Trim();
        return restaurants.Where(restaurant =>
            restaurant.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            restaurant.Specialty.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            restaurant.Location.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public Task<Restaurant?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<Restaurant> CreateAsync(CreateRestaurantDto dto, User requestor)
    {
        ValidateOwner(requestor);
        if (requestor.Restaurant is not null)
            throw new ArgumentException("Ya tienes un restaurante asociado. Puedes editar su perfil.");

        ValidateProfile(dto.Name, dto.Specialty, dto.Location);
        var restaurant = new Restaurant
        {
            Name = dto.Name.Trim(),
            Specialty = dto.Specialty.Trim(),
            Location = dto.Location.Trim(),
            Description = dto.Description?.Trim()
        };

        await _repository.AddAsync(restaurant);
        requestor.SetRestaurant(restaurant);
        _userRepository.Update(requestor);
        return restaurant;
    }

    public async Task UpdateAsync(UpdateRestaurantDto dto, User requestor)
    {
        ValidateOwnership(dto.Id, requestor);
        var restaurant = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("Restaurante no encontrado.");

        ValidateProfile(dto.Name, dto.Specialty, dto.Location);
        restaurant.Name = dto.Name.Trim();
        restaurant.Specialty = dto.Specialty.Trim();
        restaurant.Location = dto.Location.Trim();
        restaurant.Description = dto.Description?.Trim();
        await _repository.UpdateAsync(restaurant);
    }

    public async Task DeleteAsync(Guid id, User requestor)
    {
        ValidateOwnership(id, requestor);
        await _repository.DeleteAsync(id);
    }

    private static void ValidateOwner(User requestor)
    {
        if (requestor is null || requestor.Role != Roles.Propietary || !requestor.IsActive)
            throw new UnauthorizedAccessException("Se requiere una cuenta de propietario activa.");
    }

    private static void ValidateOwnership(Guid restaurantId, User requestor)
    {
        ValidateOwner(requestor);
        if (requestor.Restaurant is null || requestor.Restaurant.Id != restaurantId)
            throw new UnauthorizedAccessException("Solo puedes administrar tu propio restaurante.");
    }

    private static void ValidateProfile(string name, string specialty, string location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("La especialidad es obligatoria.");
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("La ubicación es obligatoria.");
    }
}
