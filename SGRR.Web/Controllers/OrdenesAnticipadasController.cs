using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;
using SGRR.Web.Services;

namespace SGRR.Web.Controllers
{
    public class OrdenesAnticipadasController : Controller
    {
        private readonly IOrdenesAnticipadasRepository _ordenesRepository;
        private readonly IDishRepository _dishRepository;

        public OrdenesAnticipadasController(
            IOrdenesAnticipadasRepository ordenesRepository,
            IDishRepository dishRepository)
        {
            _ordenesRepository = ordenesRepository;
            _dishRepository = dishRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarProductosViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(OrdenAnticipada orden)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Por favor, complete todos los campos requeridos.";
                await CargarProductosViewBag();
                return View("Index", orden);
            }

            _ordenesRepository.Agregar(orden);
            ViewBag.SuccessMessage = "¡Orden anticipada registrada exitosamente!";

            await CargarProductosViewBag();
            return View("Index");
        }

        private async Task CargarProductosViewBag()
        {
            var platillos = await _dishRepository.GetAllAsync();

            // Si no hay platillos registrados en la BD, agregamos opciones por defecto para prueba
            if (platillos == null || !platillos.Any())
            {
                ViewBag.Productos = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Plato del Día" },
            new SelectListItem { Value = "2", Text = "Menú Ejecutivo" },
            new SelectListItem { Value = "3", Text = "Combo Especial" }
        };
            }
            else
            {
                ViewBag.Productos = platillos.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();
            }
        }
    }
}