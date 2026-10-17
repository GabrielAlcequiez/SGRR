using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Controllers;

public class ClientController : Controller
{
    private readonly IRestaurantSearchService _restaurantSearchService;
    private readonly IUserRepository _userRepository;

    public ClientController(IRestaurantSearchService restaurantSearchService, IUserRepository userRepository)
    {
        _restaurantSearchService = restaurantSearchService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(RestaurantSearchViewModel filters)
    {
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            return RedirectToAction("Login", "Auth");
        var client = _userRepository.GetById(userId);
        if (client is null)
            return RedirectToAction("Login", "Auth");
        if (client.Role != Roles.Client || !client.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (!ModelState.IsValid)
            return View(filters);

        var restaurants = await _restaurantSearchService.SearchAsync(new RestaurantSearchFilterDto
        {
            Specialty = filters.Specialty,
            Location = filters.Location,
            MinimumRating = filters.MinimumRating
        });

        filters.Results = restaurants;
        return View(filters);
    }
}
