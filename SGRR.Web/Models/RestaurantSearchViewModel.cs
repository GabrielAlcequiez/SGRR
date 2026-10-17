using System.ComponentModel.DataAnnotations;
using RestaurantEntity = SGRR.Data.Entities.Restaurant;

namespace SGRR.Web.Models;

public class RestaurantSearchViewModel
{
    public string? Specialty { get; set; }
    public string? Location { get; set; }

    [Range(typeof(decimal), "1", "5", ErrorMessage = "La evaluación mínima debe estar entre 1 y 5.")]
    public decimal? MinimumRating { get; set; }

    public IEnumerable<RestaurantEntity> Results { get; set; } = [];
}
