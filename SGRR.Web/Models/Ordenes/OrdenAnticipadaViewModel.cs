namespace SGRR.Web.Models.Ordenes;

public class OrdenAnticipadaViewModel
{
    public Guid Id { get; set; }
    public string PurchaseCode { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; }
    public DateTime FechaProgramada { get; set; }
    public TimeSpan HoraProgramada { get; set; }
    public string TipoEntrega { get; set; } = string.Empty;
    public string? DireccionEntrega { get; set; }
    public string? Notas { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<string> Productos { get; set; } = new();
}
