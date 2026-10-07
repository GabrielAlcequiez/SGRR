using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public AuthService(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
    }

    public User Register(string name, string lastName, string email, string password, Roles role)
    {
        if (_userRepository.ExistsByEmail(email))
        {
            throw new InvalidOperationException($"El correo '{email}' ya se encuentra registrado.");
        }

        return _authRepository.Register(name, lastName, email, password, role)
            ?? throw new InvalidOperationException("No se pudo registrar el usuario.");
    }

    public async Task<User> RegisterProprietaryAsync(
        string name,
        string lastName,
        string email,
        string password,
        string restaurantName,
        string specialty,
        string location,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(restaurantName) ||
            string.IsNullOrWhiteSpace(specialty) ||
            string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("El nombre, la especialidad y la ubicación del local son obligatorios.");
        }

        if (_userRepository.ExistsByEmail(email))
        {
            throw new InvalidOperationException($"El correo '{email}' ya se encuentra registrado.");
        }

        if (await _restaurantRepository.ExistsByNameAndLocationAsync(restaurantName, location))
        {
            throw new InvalidOperationException("Ya existe un local registrado con ese nombre y ubicación.");
        }

        var restaurant = new Restaurant
        {
            Name = restaurantName.Trim(),
            Specialty = specialty?.Trim() ?? string.Empty,
            Location = location.Trim(),
            Description = description?.Trim()
        };

        return _authRepository.Register(name, lastName, email, password, Roles.Propietary, restaurant)
            ?? throw new InvalidOperationException("No se pudo registrar el propietario.");
    }

    public User? Login(string email, string password)
    {
        var user = _authRepository.Login(email, password);
        return user is { IsActive: true } ? user : null;
    }
}