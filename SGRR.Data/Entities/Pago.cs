namespace SGRR.Data.Entities;

public class Pago
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrdenAnticipadaId { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = string.Empty;

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;

    public OrdenAnticipada? OrdenAnticipada { get; set; }
}