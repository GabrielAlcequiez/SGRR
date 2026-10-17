namespace SGRR.Data.Entities;

public class OrdenAnticipada
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PurchaseCode { get; set; } = string.Empty;   // "ORD-2026-0001" asignado por el servicio
    public Guid ClientId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<OrdenProducto> Productos { get; set; } = new();
    public DateTime FechaOrden { get; set; } = DateTime.Now;
    public DateTime FechaProgramada { get; set; }
    public TimeSpan HoraProgramada { get; set; }
    public string TipoEntrega { get; set; } = "Recoger";       // Recoger | Delivery
    public string? DireccionEntrega { get; set; }              // obligatoria si Delivery
    public string? Notas { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pendiente";          // Pendiente | Aceptada | Rechazada | Entregada
}

public class OrdenProducto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrdenAnticipadaId { get; set; }   // FK
    public Guid MenuItemId { get; set; }
    public string NombreProducto { get; set; } = string.Empty; // snapshot al ordenar
    public decimal PrecioUnitario { get; set; }                // snapshot al ordenar
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
