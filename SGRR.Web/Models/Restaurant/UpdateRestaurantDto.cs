using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Restaurant;

public class UpdateRestaurantDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    public string Specialty { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }
}
