using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class NotificacionController : Controller
{
    private readonly INotificacionService _notificacionService;
    private readonly IUserRepository _userRepository;

    public NotificacionController(
        INotificacionService notificacionService,
        IUserRepository userRepository)
    {
        _notificacionService = notificacionService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var client = GetCurrentClientUser();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        try
        {
            var notifications = _notificacionService.GetClientNotifications(client);
            return View(notifications);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login", "Auth");
        }
    }

    private User? GetCurrentClientUser()
    {
        if (HttpContext.Session.GetString("UserRole") != Roles.Client.ToString())
            return null;

        return Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId)
            ? _userRepository.GetById(userId)
            : null;
    }
}
