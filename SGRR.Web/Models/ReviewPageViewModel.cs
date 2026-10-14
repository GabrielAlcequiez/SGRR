using System.ComponentModel.DataAnnotations;
using SGRR.Data.Common.Enums;
using SGRR.Data.Models;

namespace SGRR.Web.Models;

public class ReviewPageViewModel
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Guid CurrentCustomerId { get; set; }
    public ReviewSortOrder SortOrder { get; set; } = ReviewSortOrder.NewestFirst;

    [Range(1, 5, ErrorMessage = "Selecciona una puntuación entre 1 y 5 estrellas.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(1000, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
    public string Comment { get; set; } = string.Empty;

    public IEnumerable<ReviewDto> Reviews { get; set; } = [];
}
