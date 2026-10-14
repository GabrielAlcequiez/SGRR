using System.ComponentModel.DataAnnotations;

namespace SGRR.Data.Models;

public class ReviewWriteDto
{
    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5 estrellas.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "El comentario no puede superar los 1000 caracteres.")]
    public string Comment { get; set; } = string.Empty;
}
