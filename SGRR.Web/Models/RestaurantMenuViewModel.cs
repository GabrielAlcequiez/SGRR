using SGRR.Data.Entities;

namespace SGRR.Web.Models
{
    public class RestaurantMenuViewModel
    {
        public Guid RestaurantId { get; set; }

        // Propiedad pública para que el controlador pueda asignarle el nombre
        public string RestaurantName { get; set; } = string.Empty;

        // Almacena los platos organizados por categorías (Ej: "Platos Fuertes", "Bebidas")
        public Dictionary<string, List<MenuItem>> CategorizedMenu { get; set; } = new();

        // Almacena las horas en formato de texto (Ej: "12:00", "12:30")
        public List<string> AvailableHours { get; set; } = new();

        // Mensajes para el manejo de errores solicitados en el PBI
        public string MenuErrorMessage { get; set; } = string.Empty;
        public string ScheduleErrorMessage { get; set; } = string.Empty;
    }
}
