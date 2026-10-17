using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public sealed class PagoService : IPagoService
{
    private readonly AppDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PagoService> _logger;

    public PagoService(
        AppDbContext context,
        IPaymentGateway paymentGateway,
        ILogger<PagoService> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public Task<OrdenAnticipada?> ObtenerOrdenPropiaAsync(
        Guid ordenId,
        Guid clienteId,
        CancellationToken cancellationToken = default)
    {
        if (ordenId == Guid.Empty || clienteId == Guid.Empty)
        {
            return Task.FromResult<OrdenAnticipada?>(null);
        }

        return _context.OrdenesAnticipadas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                orden => orden.Id == ordenId && orden.ClientId == clienteId,
                cancellationToken);
    }

    public async Task<bool> TienePagoAsync(
        Guid ordenId,
        Guid clienteId,
        CancellationToken cancellationToken = default)
    {
        var ordenPerteneceAlCliente = await _context.OrdenesAnticipadas
            .AsNoTracking()
            .AnyAsync(
                orden => orden.Id == ordenId && orden.ClientId == clienteId,
                cancellationToken);

        if (!ordenPerteneceAlCliente)
        {
            return false;
        }

        return await _context.Pagos
            .AsNoTracking()
            .AnyAsync(pago => pago.OrdenAnticipadaId == ordenId, cancellationToken);
    }

    public async Task<PagoResultadoDto> ProcesarAsync(
        Guid clienteId,
        PagoTransaccionDto transaccion,
        CancellationToken cancellationToken = default)
    {
        if (clienteId == Guid.Empty || transaccion.OrdenAnticipadaId == Guid.Empty)
        {
            return Rechazar("No se pudo identificar al cliente o a la orden.");
        }

        var orden = await _context.OrdenesAnticipadas
            .FirstOrDefaultAsync(
                elemento => elemento.Id == transaccion.OrdenAnticipadaId &&
                            elemento.ClientId == clienteId,
                cancellationToken);

        if (orden is null)
        {
            _logger.LogWarning(
                "Intento de pago denegado: cliente {ClienteId}, orden {OrdenId}.",
                clienteId,
                transaccion.OrdenAnticipadaId);

            return Rechazar("No se encontró la orden o no tienes permiso para pagarla.");
        }

        if (orden.Total <= 0)
        {
            return Rechazar("El total de la orden debe ser mayor que cero.");
        }

        if (transaccion.Monto != orden.Total)
        {
            _logger.LogWarning(
                "Importe de pago no coincidente: cliente {ClienteId}, orden {OrdenId}.",
                clienteId,
                orden.Id);

            return Rechazar("El importe del pago no coincide con el total actual de la orden.");
        }

        var yaTienePago = await _context.Pagos
            .AsNoTracking()
            .AnyAsync(pago => pago.OrdenAnticipadaId == orden.Id, cancellationToken);

        if (yaTienePago)
        {
            return Rechazar("Esta orden ya tiene un pago registrado.");
        }

        PaymentGatewayResult resultadoPasarela;

        try
        {
            resultadoPasarela = await _paymentGateway.ProcesarAsync(
                transaccion,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _logger.LogError(
                "Fallo de la pasarela simulada para la orden {OrdenId} y el cliente {ClienteId}.",
                orden.Id,
                clienteId);

            return Rechazar("No se pudo completar la comunicación con la pasarela de pago.");
        }

        if (!resultadoPasarela.Aprobado)
        {
            _logger.LogWarning(
                "Pago rechazado: cliente {ClienteId}, orden {OrdenId}, importe {Monto}, método {MetodoPago}, referencia {Referencia}.",
                clienteId,
                orden.Id,
                orden.Total,
                transaccion.MetodoPago,
                resultadoPasarela.Referencia);

            return Rechazar(resultadoPasarela.Mensaje);
        }

        var pago = new Pago
        {
            OrdenAnticipadaId = orden.Id,
            Monto = orden.Total,
            MetodoPago = transaccion.MetodoPago,
            FechaPago = DateTime.UtcNow
        };

        _context.Pagos.Add(pago);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _logger.LogError(
                "La pasarela aprobó el pago, pero no se pudo guardar para la orden {OrdenId} y el cliente {ClienteId}.",
                orden.Id,
                clienteId);

            return Rechazar("El pago fue autorizado, pero no se pudo registrar. Contacta con soporte.");
        }

        _logger.LogInformation(
            "Pago registrado: pago {PagoId}, cliente {ClienteId}, orden {OrdenId}, importe {Monto}, método {MetodoPago}, referencia {Referencia}.",
            pago.Id,
            clienteId,
            orden.Id,
            pago.Monto,
            pago.MetodoPago,
            resultadoPasarela.Referencia);

        return new PagoResultadoDto(
            true,
            "Pago realizado correctamente.",
            pago.Id);
    }

    private static PagoResultadoDto Rechazar(string mensaje)
    {
        return new PagoResultadoDto(false, mensaje);
    }
}