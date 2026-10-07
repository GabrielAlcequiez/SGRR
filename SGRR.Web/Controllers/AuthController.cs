using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Auth;

namespace SGRR.Web.Controllers;

public class AuthController : Controller
{
    private const string UserIdSessionKey = "UserId";
    private const string UserNameSessionKey = "UserName";
    private const string UserRoleSessionKey = "UserRole";
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterClientDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        if (!TryGetAllowedRole(dto.Role, out var role))
        {
            ModelState.AddModelError(nameof(dto.Role), "Selecciona un tipo de usuario válido.");
            return View(dto);
        }

        if (role == Roles.Propietary &&
            (string.IsNullOrWhiteSpace(dto.RestaurantName) ||
             string.IsNullOrWhiteSpace(dto.Specialty) ||
             string.IsNullOrWhiteSpace(dto.Location)))
        {
            if (string.IsNullOrWhiteSpace(dto.RestaurantName))
                ModelState.AddModelError(nameof(dto.RestaurantName), "El nombre del restaurante es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Specialty))
                ModelState.AddModelError(nameof(dto.Specialty), "La especialidad es obligatoria.");
            if (string.IsNullOrWhiteSpace(dto.Location))
                ModelState.AddModelError(nameof(dto.Location), "La ubicación es obligatoria.");
            return View(dto);
        }

        try
        {
            var user = role == Roles.Propietary
                ? await _authService.RegisterProprietaryAsync(
                    dto.Name,
                    dto.LastName,
                    dto.Email,
                    dto.Password,
                    dto.RestaurantName!,
                    dto.Specialty!,
                    dto.Location!,
                    dto.Description)
                : _authService.Register(dto.Name, dto.LastName, dto.Email, dto.Password, Roles.Client);
            StartSession(user.Id, user.Name, user.Role);
            return RedirectToRoleHome(user.Role);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var user = _authService.Login(dto.Email, dto.Password);
        if (user is null || (user.Role != Roles.Client && user.Role != Roles.Propietary))
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(dto);
        }

        StartSession(user.Id, user.Name, user.Role);
        return RedirectToRoleHome(user.Role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private void StartSession(Guid userId, string name, Roles role)
    {
        HttpContext.Session.SetString(UserIdSessionKey, userId.ToString());
        HttpContext.Session.SetString(UserNameSessionKey, name);
        HttpContext.Session.SetString(UserRoleSessionKey, role.ToString());
    }

    private IActionResult RedirectToRoleHome(Roles role)
    {
        return role switch
        {
            Roles.Client => RedirectToAction("Index", "Client"),
            Roles.Propietary => RedirectToAction("Dashboard", "Proprietary"),
            _ => RedirectToAction(nameof(Login))
        };
    }

    // simple helper para validar que el rol sea el correcto y que rechaze cualquier otra cosa.
    private static bool TryGetAllowedRole(string value, out Roles role)
    {
        if (Enum.TryParse(value, ignoreCase: true, out role) &&
            (role == Roles.Client || role == Roles.Propietary))
        {
            return true;
        }

        role = default;
        return false;
    }
}