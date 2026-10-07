using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public User? Register(string name, string lastName, string email, string password, Roles role, Restaurant? restaurant = null)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Nombre, correo y contraseña son obligatorios.");
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        var newUser = new User(
            name: name.Trim(),
            lastName: lastName?.Trim() ?? string.Empty,
            email: normalizedEmail,
            password: password,
            role: role,
            restaurant: restaurant
        );

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return newUser;
    }

    public User? Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        // Buscar coincidencia de email y contraseña directa (no habra hash x ahora como es un proyecto para simular devops)
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == normalizedEmail && u.Password == password);
        return user;
    }
}
