using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly IUserRepository _userRepository;

    public CategoryController(ICategoryService categoryService, IUserRepository userRepository)
    {
        _categoryService = categoryService;
        _userRepository = userRepository;
    }

    private User? GetCurrentAdminUser()
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (roleStr != Roles.Administrator.ToString()) return null;

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            return _userRepository.GetById(userId);
        }
        return null;
    }

    public IActionResult Index()
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null) return RedirectToAction("Login", "Auth");

        try
        {
            var categories = _categoryService.GetAllCategories(adminUser);
            return View(categories);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login", "Auth");
        }
    }

    [HttpPost]
    public IActionResult Create(string name)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null) return RedirectToAction("Login", "Auth");

        try
        {
            _categoryService.CreateCategory(name, adminUser);
            TempData["SuccessMessage"] = "Categoría creada exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

[HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null) return RedirectToAction("Login", "Auth");

        try
        {
            await _categoryService.DeleteCategoryAsync(id, adminUser);
            TempData["SuccessMessage"] = "Categoría eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}