using SGRR.Data.Models;

namespace SGRR.Web.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcesarAsync(
        PagoTransaccionDto transaccion,
        CancellationToken cancellationToken = default);
}

public sealed record PaymentGatewayResult(
    bool Aprobado,
    string Mensaje,
    string Referencia);