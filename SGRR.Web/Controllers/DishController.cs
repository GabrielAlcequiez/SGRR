using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Controllers;

// Gestión del menú del propietario sobre MenuItems (la misma fuente de datos que ve el cliente)
public class DishController : OwnerController
{
    private readonly IMenuService _menuService;

    public DishController(IMenuService menuService, IUserRepository userRepository)
        : base(userRepository)
    {
        _menuService = menuService;
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
        return View(await _menuService.GetByRestaurantIdForOwnerAsync(CurrentOwner));
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
            await _menuService.CreateAsync(dto, CurrentOwner);
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
            var item = await _menuService.GetByIdAsync(id, CurrentOwner);
            if (item is null)
                return NotFound();
            return View(new UpdateDishDto
            {
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Description = item.Description,
                Category = item.Category,
                ImageUrl = item.ImageUrl
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
            if (await _menuService.GetByIdAsync(dto.Id, CurrentOwner) is null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(dto);

            await _menuService.UpdateAsync(dto, CurrentOwner);
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
            await _menuService.DeleteAsync(id, CurrentOwner);
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
