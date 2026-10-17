using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Interfaces;
using SGRR.Data.Interfaces;

namespace SGRR.Web.Controllers;

public class RestaurantMenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IRestaurantRepository _restaurants;

    public RestaurantMenuController(IMenuService menuService, IRestaurantRepository restaurants)
    {
        _menuService = menuService;
        _restaurants = restaurants;
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
        var restaurant = await _restaurants.GetByIdAsync(restaurantId);
        if (restaurant is null) return NotFound();
        viewModel.RestaurantName = restaurant.Name;

        return View(viewModel);
    }
}
