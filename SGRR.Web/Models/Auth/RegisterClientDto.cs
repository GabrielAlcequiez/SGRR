using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models.Auth;

public class RegisterClientDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Nombre del restaurante")]
    public string? RestaurantName { get; set; }

    [Display(Name = "Especialidad")]
    public string? Specialty { get; set; }

    [Display(Name = "Ubicación")]
    public string? Location { get; set; }

    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Selecciona un tipo de usuario.")]
    [Display(Name = "Tipo de usuario")]
    public string Role { get; set; } = "Client";

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}