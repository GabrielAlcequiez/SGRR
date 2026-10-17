using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Dish;

public class UpdateDishDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    // Categoría del menú (ejemplo: "Platos Fuertes", "Entradas", "Bebidas"). Si no se indica se usa "Otros".
    public string? Category { get; set; }

    // URL de imagen opcional para la vista pública del menú
    public string? ImageUrl { get; set; }
}
