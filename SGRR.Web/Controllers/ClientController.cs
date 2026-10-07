using Microsoft.AspNetCore.Mvc;

namespace SGRR.Web.Controllers;

public class ClientController : Controller
{
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserId") is null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (HttpContext.Session.GetString("UserRole") != "Client")
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }
}
