using System;
using System.Threading.Tasks;
using SGRR.Web.Models;

namespace SGRR.Web.Interfaces
{
    public interface IReservationService
    {
        Task<string> RegisterReservationAsync(CreateReservationViewModel model, Guid clientId);
        Task<bool> CancelReservationAsync(Guid reservationId);
        Task<bool> ValidateAvailabilityAsync(Guid restaurantId, DateTime date, string timeSlot, int peopleCount);
    }
}
