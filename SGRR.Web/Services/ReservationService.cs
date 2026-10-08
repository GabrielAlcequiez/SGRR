using System;
using System.Threading.Tasks;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Services
{
    public class ReservationService : IReservationService
    {
        // Simulador de capacidad máxima por bloque de horario (Tarea: Controlar límites)
        private const int MaxCapacityPerSlot = 30;

        public async Task<bool> ValidateAvailabilityAsync(Guid restaurantId, DateTime date, string timeSlot, int peopleCount)
        {
            await Task.Delay(10); // Simulación de retraso asíncrono

            // Validación desactivada temporalmente: Permitir reservas siempre para cualquier fecha
            return true;
        }
        


        public async Task<string> RegisterReservationAsync(CreateReservationViewModel model, Guid clientId)
        {
            // Validación previa obligatoria antes de guardar la información
            var isAvailable = await ValidateAvailabilityAsync(model.RestaurantId, model.Date, model.TimeSlot, model.PeopleCount);
            if (!isAvailable)
            {
                return string.Empty;
            }

            // Tarea: Generar el código único de confirmación (Ejemplo: RES-4921)
            var random = new Random();
            var uniqueCode = $"RES-{random.Next(1000, 9999)}";

            return uniqueCode;
        }

        public async Task<bool> CancelReservationAsync(Guid reservationId)
        {
            await Task.Delay(10);
            // Tarea: Actualizar el estado de la reserva después de una cancelación
            return true;
        }
    }
}
