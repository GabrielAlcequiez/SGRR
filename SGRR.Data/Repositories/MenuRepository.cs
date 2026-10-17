using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId && m.IsAvailable)
            .ToListAsync();
    }

    // Lectura completa para la gestión del propietario (incluye ítems marcados como no disponibles)
    public async Task<IEnumerable<MenuItem>> GetAllByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.MenuItems
            .Where(m => m.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<RestaurantSchedule>> GetSchedulesByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.RestaurantSchedules
            .Where(s => s.RestaurantId == restaurantId && s.IsActive)
            .ToListAsync();
    }

    // CRUD para la gestión del menú por parte del propietario (tabla única con la vista del cliente)
    public async Task<MenuItem?> GetByIdAsync(Guid id)
    {
        return await _context.MenuItems.FindAsync(id);
    }

    public async Task AddAsync(MenuItem item)
    {
        await _context.MenuItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MenuItem item)
    {
        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.MenuItems.FindAsync(id);
        if (item != null)
        {
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
