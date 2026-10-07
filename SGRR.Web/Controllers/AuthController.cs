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
    public IActionResult Register(RegisterClientDto dto)
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

        try
        {
            var user = _authService.Register(dto.Name, dto.LastName, dto.Email, dto.Password, role);
            StartSession(user.Id, user.Name, user.Role);
            return RedirectToRoleHome(user.Role);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(dto.Email), ex.Message);
            return View(dto);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginClientDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginClientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var user = _authService.Login(dto.Email, dto.Password);
        if (user is null)
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
        return role == Roles.Client
            ? RedirectToAction("Index", "Client")
            : RedirectToAction("Index", "Home");
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
