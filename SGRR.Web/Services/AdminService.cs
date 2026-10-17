using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly ISystemActivityLogRepository _logRepository;

    public AdminService(IUserRepository userRepository, ISystemActivityLogRepository logRepository)
    {
        _userRepository = userRepository;
        _logRepository = logRepository;
    }

    public IEnumerable<User> GetAllUsers(User requestor)
    {
        ValidateAdminRole(requestor);
        return _userRepository.GetAll();
    }

    public void ToggleUserStatus(Guid targetUserId, bool isActive, User requestor)
    {
        ValidateAdminRole(requestor);

        var userToUpdate = _userRepository.GetById(targetUserId);
        if (userToUpdate == null)
        {
            throw new KeyNotFoundException("No se encontró el usuario especificado.");
        }

        // Evitar que el admin se bloquee a sí mismo por accidente
        if (userToUpdate.Id == requestor.Id && !isActive)
        {
            throw new InvalidOperationException("Un administrador no puede bloquear su propia cuenta.");
        }

        userToUpdate.SetActive(isActive);
        _userRepository.Update(userToUpdate);
        _logRepository.AddLog(
            LogSeverity.Warning,
            $"El administrador cambió el estado del usuario '{userToUpdate.Email}' a {(isActive ? "Activo" : "Inactivo")}.");
    }

    private void ValidateAdminRole(User user)
    {
        if (user == null || user.Role != Roles.Administrator)
        {
            throw new UnauthorizedAccessException("Acceso denegado. Se requieren permisos de Administrador.");
        }
    }
}