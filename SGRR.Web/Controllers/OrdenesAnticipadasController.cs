using Microsoft.AspNetCore.Mvc;

namespace SGRR.Web.Controllers
{
    public class OrdenesAnticipadasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}