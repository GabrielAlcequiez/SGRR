using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class MonitoringController : Controller
{
    private readonly IPlatformMonitoringService _monitoringService;
    private readonly IUserRepository _userRepository;

    public MonitoringController(IPlatformMonitoringService monitoringService, IUserRepository userRepository)
    {
        _monitoringService = monitoringService;
        _userRepository = userRepository;
    }

    private User? GetCurrentAdminUser()
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (roleStr != Roles.Admin.ToString()) return null;

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            return _userRepository.GetById(userId);
        }
        return null;
    }

    public async Task<IActionResult> Index()
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null) return RedirectToAction("Login", "Auth");

        try
        {
            var dashboardData = await _monitoringService.GetSystemHealthAsync(adminUser);
            return View(dashboardData);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login", "Auth");
        }
    }
}