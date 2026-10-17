using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Ordenes;

public class CreateOrdenAnticipadaDto
{
    public Guid RestaurantId { get; set; }

    // Solo para mostrar en la vista; se rellena en el controlador al renderizar el formulario.
    public string RestaurantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar el tipo de entrega.")]
    public string TipoEntrega { get; set; } = "Recoger"; // "Recoger" | "Delivery"

    [DataType(DataType.Date)]
    public DateTime FechaProgramada { get; set; } = DateTime.Today;

    public TimeSpan HoraProgramada { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
    public string? DireccionEntrega { get; set; }

    [StringLength(300, ErrorMessage = "Las notas no pueden superar los 300 caracteres.")]
    public string? Notas { get; set; }

    public List<ItemOrdenDto> Items { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
