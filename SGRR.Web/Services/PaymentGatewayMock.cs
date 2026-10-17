using System.Globalization;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public sealed class PaymentGatewayMock : IPaymentGateway
{
    public Task<PaymentGatewayResult> ProcesarAsync(
        PagoTransaccionDto transaccion,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (transaccion.Monto <= 0)
        {
            return Rechazar("El importe del pago no es válido.");
        }

        if (transaccion.MetodoPago is not ("Credito" or "Debito"))
        {
            return Rechazar("Selecciona un método de pago válido.");
        }

        var numeroTarjeta = NormalizarNumeroTarjeta(transaccion.NumeroTarjeta);

        if (numeroTarjeta.Length is < 13 or > 19 ||
            numeroTarjeta.Any(caracter => caracter is < '0' or > '9'))
        {
            return Rechazar("El número de tarjeta debe contener entre 13 y 19 dígitos.");
        }

        if (!PasaValidacionLuhn(numeroTarjeta))
        {
            return Rechazar("El número de tarjeta no es válido.");
        }

        if (!EsFechaExpiracionValida(transaccion.FechaExpiracion))
        {
            return Rechazar("La tarjeta está caducada o la fecha de expiración no es válida.");
        }

        if (transaccion.Cvv.Length is < 3 or > 4 ||
            transaccion.Cvv.Any(caracter => caracter is < '0' or > '9'))
        {
            return Rechazar("El CVV debe contener 3 o 4 dígitos.");
        }

        if (numeroTarjeta.EndsWith("0000", StringComparison.Ordinal))
        {
            return Task.FromResult(new PaymentGatewayResult(
                false,
                "El banco simulado ha rechazado la tarjeta.",
                $"SIM-{Guid.NewGuid():N}"));
        }

        return Task.FromResult(new PaymentGatewayResult(
            true,
            "El pago ha sido aprobado por la pasarela simulada.",
            $"SIM-{Guid.NewGuid():N}"));
    }

    private static string NormalizarNumeroTarjeta(string? numeroTarjeta)
    {
        if (string.IsNullOrWhiteSpace(numeroTarjeta))
        {
            return string.Empty;
        }

        return new string(numeroTarjeta
            .Where(caracter => !char.IsWhiteSpace(caracter) && caracter != '-')
            .ToArray());
    }

    private static bool PasaValidacionLuhn(string numero)
    {
        var suma = 0;
        var duplicar = false;

        for (var indice = numero.Length - 1; indice >= 0; indice--)
        {
            var digito = numero[indice] - '0';

            if (duplicar)
            {
                digito *= 2;
                if (digito > 9)
                {
                    digito -= 9;
                }
            }

            suma += digito;
            duplicar = !duplicar;
        }

        return suma % 10 == 0;
    }

    private static bool EsFechaExpiracionValida(string? fechaExpiracion)
    {
        if (string.IsNullOrWhiteSpace(fechaExpiracion))
        {
            return false;
        }

        var partes = fechaExpiracion.Split('/');
        if (partes.Length != 2 ||
            !int.TryParse(partes[0], NumberStyles.None, CultureInfo.InvariantCulture, out var mes) ||
            !int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var anio) ||
            mes is < 1 or > 12 ||
            partes[1].Length != 2)
        {
            return false;
        }

        anio += 2000;

        var hoy = DateTime.UtcNow;
        return anio > hoy.Year || (anio == hoy.Year && mes >= hoy.Month);
    }

    private static Task<PaymentGatewayResult> Rechazar(string mensaje)
    {
        return Task.FromResult(new PaymentGatewayResult(
            false,
            mensaje,
            $"SIM-{Guid.NewGuid():N}"));
    }
}