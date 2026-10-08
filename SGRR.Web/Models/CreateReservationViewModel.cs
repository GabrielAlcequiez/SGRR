using System;
using System.ComponentModel.DataAnnotations;

namespace SGRR.Web.Models
{
    public class CreateReservationViewModel
    {
        public Guid RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de reserva es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El horario de reserva es obligatorio.")]
        public string TimeSlot { get; set; } = string.Empty; // Ej: "18:30 Hs"

        [Required(ErrorMessage = "La cantidad de personas es obligatoria.")]
        [Range(1, 20, ErrorMessage = "La reserva debe ser entre 1 y 20 personas.")]
        public int PeopleCount { get; set; }

        [Required(ErrorMessage = "Debe seleccionar la ubicación de la mesa.")]
        public string TableLocation { get; set; } = string.Empty; // Ej: "Terraza", "VIP", "Salón Principal"

        public string? ErrorMessage { get; set; }
    }
}
