using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IUserRepository _userRepository;

    // Inyectamos también IUserRepository para poder buscar al admin en la base de datos usando el ID de la sesión
    public AdminController(IAdminService adminService, IUserRepository userRepository)
    {
        _adminService = adminService;
        _userRepository = userRepository;
    }

    // Método privado para obtener el usuario real de la sesión
    private User? GetCurrentAdminUser()
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (roleStr != Roles.Administrator.ToString())
        {
            return null; // No es admin
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            return _userRepository.GetById(userId);
        }

        return null;
    }

    // GET: Admin/Dashboard
    public IActionResult Dashboard()
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            // Si no hay sesión o no es admin, lo pateamos al Login
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var users = _adminService.GetAllUsers(adminUser);
            return View(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login", "Auth");
        }
    }

    // POST: Admin/ToggleStatus
    [HttpPost]
    public IActionResult ToggleStatus(Guid userId, bool isActive)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            _adminService.ToggleUserStatus(userId, isActive, adminUser);
            TempData["SuccessMessage"] = isActive ? "Usuario habilitado correctamente." : "Usuario bloqueado correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}