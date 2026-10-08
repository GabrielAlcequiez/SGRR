using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Controllers;

public class DishController : OwnerController
{
    private readonly IDishService _dishService;

    public DishController(IDishService dishService, IUserRepository userRepository)
        : base(userRepository)
    {
        _dishService = dishService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        if (context.Result is not null)
            return;
        if (CurrentOwner.Restaurant is null)
        {
            TempData["ErrorMessage"] = "Primero debes crear el perfil de tu restaurante.";
            context.Result = RedirectToAction("Dashboard", "Proprietary");
            return;
        }
        ViewBag.RestaurantName = CurrentOwner.Restaurant.Name;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _dishService.GetByRestaurantIdAsync(CurrentOwner));
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateDishDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDishDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _dishService.CreateAsync(dto, CurrentOwner);
            TempData["SuccessMessage"] = "Plato agregado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var dish = await _dishService.GetByIdAsync(id, CurrentOwner);
            if (dish is null)
                return NotFound();
            return View(new UpdateDishDto
            {
                Id = dish.Id,
                Name = dish.Name,
                Price = dish.Price,
                Description = dish.Description
            });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateDishDto dto)
    {
        try
        {
            // Comprobar pertenencia incluso si el formulario contiene datos inválidos.
            if (await _dishService.GetByIdAsync(dto.Id, CurrentOwner) is null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(dto);

            await _dishService.UpdateAsync(dto, CurrentOwner);
            TempData["SuccessMessage"] = "Plato actualizado correctamente.";
            return RedirectToAction(nameof(Index));
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
            await _dishService.DeleteAsync(id, CurrentOwner);
            TempData["SuccessMessage"] = "Plato eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }
}
