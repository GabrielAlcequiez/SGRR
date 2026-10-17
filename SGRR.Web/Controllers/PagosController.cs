using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;
using SGRR.Web.ViewModels;

namespace SGRR.Web.Controllers;

public sealed class PagosController : Controller
{
    private readonly IPagoService _pagoService;
    private readonly IUserRepository _users;
    private readonly ILogger<PagosController> _logger;

    public PagosController(
        IPagoService pagoService,
        IUserRepository users,
        ILogger<PagosController> logger)
    {
        _pagoService = pagoService;
        _users = users;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid id,
        CancellationToken cancellationToken)
    {
        var respuestaSesion = ValidarClienteSesion(out var clienteId);
        if (respuestaSesion is not null)
        {
            return respuestaSesion;
        }

        var orden = await _pagoService.ObtenerOrdenPropiaAsync(
            id,
            clienteId,
            cancellationToken);

        if (orden is null)
        {
            TempData["PagoError"] = "No se encontró la orden o no tienes permiso para pagarla.";
            return RedirectToAction("MisOrdenes", "OrdenesAnticipadas");
        }

        if (await _pagoService.TienePagoAsync(id, clienteId, cancellationToken))
        {
            TempData["PagoError"] = "Esta orden ya tiene un pago registrado.";
            return RedirectToAction("MisOrdenes", "OrdenesAnticipadas");
        }

        return View(new ProcesarPagoViewModel
        {
            OrdenAnticipadaId = orden.Id,
            PurchaseCode = orden.PurchaseCode,
            MontoTotal = orden.Total
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar(
        Guid id,
        ProcesarPagoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var respuestaSesion = ValidarClienteSesion(out var clienteId);
        if (respuestaSesion is not null)
        {
            LimpiarDatosTarjeta(viewModel);
            return respuestaSesion;
        }

        var orden = await _pagoService.ObtenerOrdenPropiaAsync(
            id,
            clienteId,
            cancellationToken);

        if (orden is null)
        {
            LimpiarDatosTarjeta(viewModel);
            TempData["PagoError"] = "No se encontró la orden o no tienes permiso para pagarla.";
            return RedirectToAction("MisOrdenes", "OrdenesAnticipadas");
        }

        if (await _pagoService.TienePagoAsync(id, clienteId, cancellationToken))
        {
            LimpiarDatosTarjeta(viewModel);
            TempData["PagoError"] = "Esta orden ya tiene un pago registrado.";
            return RedirectToAction("MisOrdenes", "OrdenesAnticipadas");
        }

        viewModel.OrdenAnticipadaId = orden.Id;
        viewModel.PurchaseCode = orden.PurchaseCode;
        viewModel.MontoTotal = orden.Total;

        if (!ModelState.IsValid)
        {
            LimpiarDatosTarjeta(viewModel);
            ActualizarModelStateOrden(viewModel);
            return View("Index", viewModel);
        }

        var transaccion = new PagoTransaccionDto
        {
            OrdenAnticipadaId = orden.Id,
            Monto = orden.Total,
            MetodoPago = viewModel.MetodoPago,
            NumeroTarjeta = viewModel.NumeroTarjeta,
            FechaExpiracion = viewModel.FechaExpiracion,
            Cvv = viewModel.Cvv
        };

        PagoResultadoDto resultado;

        try
        {
            resultado = await _pagoService.ProcesarAsync(
                clienteId,
                transaccion,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            LimpiarDatosTarjeta(viewModel);
            throw;
        }
        catch
        {
            _logger.LogError(
                "Error inesperado al procesar el pago de la orden {OrdenId} para el cliente {ClienteId}.",
                orden.Id,
                clienteId);

            ModelState.AddModelError(
                string.Empty,
                "No se pudo procesar el pago. Inténtalo de nuevo.");

            LimpiarDatosTarjeta(viewModel);
            ActualizarModelStateOrden(viewModel);
            return View("Index", viewModel);
        }

        LimpiarDatosTarjeta(viewModel);

        if (!resultado.Aprobado)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            ActualizarModelStateOrden(viewModel);
            return View("Index", viewModel);
        }

        TempData["PagoExito"] = $"Pago de la orden {orden.PurchaseCode} realizado correctamente.";
        return RedirectToAction("MisOrdenes", "OrdenesAnticipadas");
    }

    private IActionResult? ValidarClienteSesion(out Guid clienteId)
    {
        clienteId = Guid.Empty;

        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            TempData["PagoError"] = "Inicia sesión como cliente para realizar el pago.";
            return RedirectToAction("Login", "Auth");
        }

        var user = _users.GetById(userId);
        if (user is null)
        {
            TempData["PagoError"] = "No se pudo validar la sesión. Inicia sesión de nuevo.";
            return RedirectToAction("Login", "Auth");
        }

        if (user.Role != Roles.Client || !user.IsActive)
        {
            TempData["PagoError"] = "Solo un cliente con una cuenta activa puede realizar pagos.";
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        clienteId = user.Id;
        return null;
    }

    private void LimpiarDatosTarjeta(ProcesarPagoViewModel viewModel)
    {
        viewModel.NumeroTarjeta = string.Empty;
        viewModel.FechaExpiracion = string.Empty;
        viewModel.Cvv = string.Empty;

        ModelState.Remove(nameof(viewModel.NumeroTarjeta));
        ModelState.Remove(nameof(viewModel.FechaExpiracion));
        ModelState.Remove(nameof(viewModel.Cvv));
    }

    private void ActualizarModelStateOrden(ProcesarPagoViewModel viewModel)
    {
        ModelState.Remove(nameof(viewModel.OrdenAnticipadaId));
        ModelState.Remove(nameof(viewModel.PurchaseCode));
        ModelState.Remove(nameof(viewModel.MontoTotal));
    }
}