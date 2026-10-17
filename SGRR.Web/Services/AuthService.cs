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
    private readonly ISystemActivityLogRepository _logRepository;

    public AuthService(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        IRestaurantRepository restaurantRepository,
        ISystemActivityLogRepository logRepository)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
        _restaurantRepository = restaurantRepository;
        _logRepository = logRepository;
    }

    public User Register(string name, string lastName, string email, string password, Roles role)
    {
        if (_userRepository.ExistsByEmail(email))
        {
            throw new InvalidOperationException($"El correo '{email}' ya se encuentra registrado.");
        }

        var user = _authRepository.Register(name, lastName, email, password, role)
            ?? throw new InvalidOperationException("No se pudo registrar el usuario.");

        // Registrar evento de auditoría
        _logRepository.AddLog(LogSeverity.Info, $"Nuevo usuario registrado: {email} (Rol: {role}).");

        return user;
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

        var user = _authRepository.Register(name, lastName, email, password, Roles.Propietary, restaurant)
            ?? throw new InvalidOperationException("No se pudo registrar el propietario.");

        // Registrar evento de auditoría
        _logRepository.AddLog(LogSeverity.Info, $"Nuevo propietario registrado: {email} con el restaurante '{restaurant.Name}'.");

        return user;
    }

    public User? Login(string email, string password)
    {
        var user = _authRepository.Login(email, password);
        
        if (user != null)
        {
            if (!user.IsActive)
            {
                // Registrar intento bloqueado
                _logRepository.AddLog(LogSeverity.Warning, $"Intento de inicio de sesión denegado. La cuenta '{email}' está bloqueada.");
                throw new UnauthorizedAccessException("Su cuenta ha sido bloqueada. Contacte al administrador.");
            }

            // Registrar inicio de sesión exitoso
            _logRepository.AddLog(LogSeverity.Info, $"El usuario '{email}' inició sesión en el sistema.");
        }

        return user;
    }
}