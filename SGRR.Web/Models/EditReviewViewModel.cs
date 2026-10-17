using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models;

public class EditReviewViewModel
{
    public Guid ReviewId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Selecciona una puntuación entre 1 y 5 estrellas.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
    public string Comment { get; set; } = string.Empty;
}
