using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Controllers;

public class DishController : Controller
{
    private readonly IDishService _dishService;

    public DishController(IDishService dishService)
    {
        _dishService = dishService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid restaurantId)
    {
        var dishes = await _dishService.GetByRestaurantIdAsync(restaurantId);
        ViewBag.RestaurantId = restaurantId;
        return View(dishes);
    }

    [HttpGet]
    public IActionResult Create(Guid restaurantId)
    {
        return View(new CreateDishDto { RestaurantId = restaurantId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDishDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _dishService.CreateAsync(dto);
            return RedirectToAction(nameof(Index), new { restaurantId = dto.RestaurantId });
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
        var dish = await _dishService.GetByIdAsync(id);
        if (dish is null)
        {
            return NotFound();
        }

        var dto = new UpdateDishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            Price = dish.Price,
            Description = dish.Description
        };

        ViewBag.RestaurantId = dish.RestaurantId;
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDishDto dto, Guid restaurantId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RestaurantId = restaurantId;
            return View(dto);
        }

        try
        {
            await _dishService.UpdateAsync(dto);
            return RedirectToAction(nameof(Index), new { restaurantId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.RestaurantId = restaurantId;
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid restaurantId)
    {
        await _dishService.DeleteAsync(id);
        return RedirectToAction(nameof(Index), new { restaurantId });
    }
}
