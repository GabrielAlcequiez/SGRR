namespace SGRR.Data.Models;

public sealed class PagoTransaccionDto
{
    public Guid OrdenAnticipadaId { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public string NumeroTarjeta { get; set; } = string.Empty;

    public string FechaExpiracion { get; set; } = string.Empty;

    public string Cvv { get; set; } = string.Empty;
}