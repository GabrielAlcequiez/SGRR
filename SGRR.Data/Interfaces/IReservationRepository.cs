using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(Guid id);
        Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid clientId);
        Task<bool> CreateAsync(Reservation reservation);
        Task<bool> UpdateAsync(Reservation reservation);
        Task<int> GetTotalReservedCapacityAsync(Guid restaurantId, DateTime date, string timeSlot);
        Task<bool> HasDuplicateReservationAsync(Guid clientId, DateTime date, string timeSlot);
    }
}
