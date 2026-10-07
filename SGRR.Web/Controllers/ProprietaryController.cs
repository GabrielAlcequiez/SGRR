using Microsoft.AspNetCore.Mvc;

namespace SGRR.Web.Controllers;

public class ProprietaryController : Controller
{
    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("UserId") is null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (HttpContext.Session.GetString("UserRole") != "Propietary")
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
}
