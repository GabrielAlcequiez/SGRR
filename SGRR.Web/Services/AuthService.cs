using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;

    public AuthService(IAuthRepository authRepository, IUserRepository userRepository)
    {
        _authRepository = authRepository;
        _userRepository = userRepository;
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

    public User? Login(string email, string password)
    {
        return _authRepository.Login(email, password);
    }
}
