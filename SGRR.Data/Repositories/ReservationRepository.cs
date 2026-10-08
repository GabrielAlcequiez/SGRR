using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> GetByIdAsync(Guid id)
        {
            return await _context.Reservations.FindAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetByClientIdAsync(Guid clientId)
        {
            return await _context.Reservations
                .Where(r => r.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetTotalReservedCapacityAsync(Guid restaurantId, DateTime date, string timeSlot)
        {
            // Sumar la cantidad de personas de las reservas activas en ese horario específico
            return await _context.Reservations
                .Where(r => r.RestaurantId == restaurantId &&
                            r.Date.Date == date.Date &&
                            r.TimeSlot == timeSlot &&
                            r.Status == "Active")
                .SumAsync(r => r.PeopleCount);
        }

        public async Task<bool> HasDuplicateReservationAsync(Guid clientId, DateTime date, string timeSlot)
        {
            // Validar que el mismo cliente no duplique reservas para el mismo horario
            return await _context.Reservations
                .AnyAsync(r => r.ClientId == clientId &&
                               r.Date.Date == date.Date &&
                               r.TimeSlot == timeSlot &&
                               r.Status == "Active");
        }
    }
}
