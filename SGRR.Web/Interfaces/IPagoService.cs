using SGRR.Data.Entities;
using SGRR.Data.Models;

namespace SGRR.Web.Interfaces;

public interface IPagoService
{
    Task<OrdenAnticipada?> ObtenerOrdenPropiaAsync(
        Guid ordenId,
        Guid clienteId,
        CancellationToken cancellationToken = default);

    Task<bool> TienePagoAsync(
        Guid ordenId,
        Guid clienteId,
        CancellationToken cancellationToken = default);

    Task<PagoResultadoDto> ProcesarAsync(
        Guid clienteId,
        PagoTransaccionDto transaccion,
        CancellationToken cancellationToken = default);
}