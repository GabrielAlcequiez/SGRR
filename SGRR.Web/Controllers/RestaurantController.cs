using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Controllers;

public class RestaurantController : OwnerController
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService, IUserRepository userRepository)
        : base(userRepository)
    {
        _restaurantService = restaurantService;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction("Dashboard", "Proprietary");

    [HttpGet]
    public IActionResult Create()
    {
        return CurrentOwner.Restaurant is not null
            ? RedirectToAction("Dashboard", "Proprietary")
            : View(new CreateRestaurantDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRestaurantDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _restaurantService.CreateAsync(dto, CurrentOwner);
            TempData["SuccessMessage"] = "Perfil del restaurante creado correctamente.";
            return RedirectToAction("Dashboard", "Proprietary");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (CurrentOwner.Restaurant?.Id != id)
            return StatusCode(StatusCodes.Status403Forbidden);

        var restaurant = await _restaurantService.GetByIdAsync(id);
        if (restaurant is null)
            return NotFound();

        return View(new UpdateRestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Specialty = restaurant.Specialty,
            Location = restaurant.Location,
            Description = restaurant.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateRestaurantDto dto)
    {
        if (CurrentOwner.Restaurant?.Id != dto.Id)
            return StatusCode(StatusCodes.Status403Forbidden);
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _restaurantService.UpdateAsync(dto, CurrentOwner);
            TempData["SuccessMessage"] = "Perfil del restaurante actualizado correctamente.";
            return RedirectToAction("Dashboard", "Proprietary");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _restaurantService.DeleteAsync(id, CurrentOwner);
            TempData["SuccessMessage"] = "Restaurante eliminado correctamente.";
            return RedirectToAction("Dashboard", "Proprietary");
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }
}
