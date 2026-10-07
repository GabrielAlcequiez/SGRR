using SGRR.Data.Common.Enums;

namespace SGRR.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; private set; }
    public string LastName { get; private set; } 
    public string Email { get; private set; } 
    public string Password { get; private set; }
    public Roles Role  { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Restaurant? Restaurant { get; private set; }

    public User(
        string name,
        string lastName,
        string email,
        string password,
        Roles role)
    {
        // Validations
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre es requerido.", nameof(name));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("El email no es válido.", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña es requerida.", nameof(password));

        Name = name.Trim();
        LastName = lastName?.Trim() ?? string.Empty;
        Email = email.Trim().ToLowerInvariant();
        Password = password;
        Role = role;
    }

    public void SetRestaurant(Restaurant restaurant)
    {
        ArgumentNullException.ThrowIfNull(restaurant);
        Restaurant = restaurant;
    }

    // Añadido para manejar el bloqueo de cuentas.
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
