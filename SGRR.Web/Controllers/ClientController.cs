using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class ClientController : Controller
{
    private readonly IRestaurantService _restaurantService;
    private readonly IUserRepository _userRepository;

    public ClientController(IRestaurantService restaurantService, IUserRepository userRepository)
    {
        _restaurantService = restaurantService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search)
    {
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
            return RedirectToAction("Login", "Auth");
        var client = _userRepository.GetById(userId);
        if (client is null)
            return RedirectToAction("Login", "Auth");
        if (client.Role != Roles.Client || !client.IsActive)
            return StatusCode(StatusCodes.Status403Forbidden);

        ViewBag.Search = search;
        return View(await _restaurantService.SearchAsync(search));
    }
}
