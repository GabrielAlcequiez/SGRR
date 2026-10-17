using Microsoft.AspNetCore.Mvc;
using SGRR.Web.Models;
using SGRR.Web.Services;

namespace SGRR.Web.Controllers
{
    public class OrdenesAnticipadasController : Controller
    {
        private readonly IOrdenesAnticipadasRepository _ordenesRepository;

        public OrdenesAnticipadasController(IOrdenesAnticipadasRepository ordenesRepository)
        {
            _ordenesRepository = ordenesRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(SGRR.Web.Models.OrdenAnticipada orden)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Por favor, complete todos los campos requeridos.";
                return View("Index", orden);
            }

            _ordenesRepository.Agregar(orden);
            ViewBag.SuccessMessage = "¡Orden anticipada registrada exitosamente!";
            return View("Index");
        }
    }
}