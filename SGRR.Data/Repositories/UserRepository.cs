using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        // Validación de email
        if (ExistsByEmail(user.Email))
        {
            throw new InvalidOperationException("Ya existe un usuario registrado con ese email");
        }
        
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public void Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var existingUser = GetById(user.Id);
        if (existingUser is null)
        {
            throw new KeyNotFoundException($"No se encontro el usuario {user.Email}");
        }
        
        // validación por si cambia email
        bool emailTakenByOther = _context.Users.Any(x => x.Id != user.Id && x.Email.ToLower() == user.Email.Trim().ToLower());
        if (emailTakenByOther)
        {
            throw new InvalidOperationException($"El correo '{user.Email}' ya está en uso por otro usuario.");
        }
        
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        _context.SaveChanges();
    }

    public IEnumerable<User> GetAll()
    {
        return [.. _context.Users];
    }

    public User? GetById(Guid id)
    {
        return _context.Users.Find(id);
    }

    public bool ExistsByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        
        string normalizedEmail = email.Trim().ToLowerInvariant();
        return _context.Users.Any(x => x.Email.ToLower() == normalizedEmail);
    }
}
