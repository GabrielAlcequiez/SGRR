using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Controllers;

// Gestión de órdenes anticipadas del restaurante del propietario autenticado (RF05/RF12).
public class OwnerOrdersController : OwnerController
{
    private readonly IOrdenAnticipadaService _service;

    public OwnerOrdersController(IOrdenAnticipadaService service, IUserRepository userRepository)
        : base(userRepository)
    {
        _service = service;
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
        return View(await _service.GetByRestaurantIdAsync(CurrentOwner.Restaurant!.Id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Aceptar(Guid id) =>
        VolverAlListado(await _service.AceptarAsync(id, CurrentOwner.Restaurant!.Id),
            "Orden aceptada correctamente. El cliente fue notificado.",
            "No se pudo aceptar la orden: solo las órdenes pendientes pueden aceptarse.");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(Guid id) =>
        VolverAlListado(await _service.RechazarAsync(id, CurrentOwner.Restaurant!.Id),
            "Orden rechazada correctamente. El cliente fue notificado.",
            "No se pudo rechazar la orden: solo las órdenes pendientes pueden rechazarse.");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarEntregada(Guid id) =>
        VolverAlListado(await _service.MarcarEntregadaAsync(id, CurrentOwner.Restaurant!.Id),
            "Orden marcada como entregada.",
            "No se pudo marcar como entregada: solo las órdenes aceptadas pueden entregarse.");

    private IActionResult VolverAlListado(bool ok, string mensajeExito, string mensajeError)
    {
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? mensajeExito : mensajeError;
        return RedirectToAction(nameof(Index));
    }
}
