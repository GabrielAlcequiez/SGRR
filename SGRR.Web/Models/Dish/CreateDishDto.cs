using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Dish;

public class CreateDishDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    public decimal Price { get; set; }

    public string? Description { get; set; }
}
