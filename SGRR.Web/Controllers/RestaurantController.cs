using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Controllers;

public class RestaurantController : Controller
{
    private readonly IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var restaurants = await _restaurantService.GetAllAsync();
        return View(restaurants);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateRestaurantDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRestaurantDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _restaurantService.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
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
        var restaurant = await _restaurantService.GetByIdAsync(id);
        if (restaurant is null)
        {
            return NotFound();
        }

        var dto = new UpdateRestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Specialty = restaurant.Specialty,
            Location = restaurant.Location,
            Description = restaurant.Description
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateRestaurantDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _restaurantService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
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
        await _restaurantService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
