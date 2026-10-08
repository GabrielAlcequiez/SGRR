using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class RestaurantMenuController : Controller
{
    private readonly IMenuService _menuService;

    public RestaurantMenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    // Tu endpoint de consulta: GET /RestaurantMenu/Index?restaurantId=...&date=...
    public async Task<IActionResult> Index(Guid restaurantId, DateTime? date)
    {
        if (restaurantId == Guid.Empty)
        {
            return BadRequest("El ID del restaurante especificado no es válido.");
        }

        DateTime selectedDate = date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

        var viewModel = await _menuService.GetMenuAndSchedulesAsync(restaurantId, selectedDate);
        viewModel.RestaurantName = "El Sazón Dominicano";

        return View(viewModel);
    }
}
