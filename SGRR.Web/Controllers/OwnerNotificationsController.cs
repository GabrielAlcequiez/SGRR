using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class OwnerNotificationsController : Controller
{
    private readonly IOwnerNotificationService _notificationService;
    private readonly IRestaurantRepository _restaurantRepository;

    public OwnerNotificationsController(
        IOwnerNotificationService notificationService,
        IRestaurantRepository restaurantRepository)
    {
        _notificationService = notificationService;
        _restaurantRepository = restaurantRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (roleStr != Roles.Propietary.ToString() && roleStr != "Propietary")
        {
            return RedirectToAction("Login", "Auth");
        }

        // Obtenemos el restaurante asociado al propietario logueado (usando la lógica de respaldo que dejamos en el dashboard)
        var restaurants = await _restaurantRepository.GetAllAsync();
        var restaurant = restaurants.FirstOrDefault(); // O filtrado por usuario si aplica

        if (restaurant == null)
        {
            return View(new List<SGRR.Data.Entities.Notification>());
        }

        var notifications = await _notificationService.GetOwnerNotificationsAsync(restaurant.Id);
        return View(notifications);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return RedirectToAction(nameof(Index));
    }
}