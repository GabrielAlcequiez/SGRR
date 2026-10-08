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

    public async Task<IEnumerable<RestaurantSchedule>> GetSchedulesByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.RestaurantSchedules
            .Where(s => s.RestaurantId == restaurantId && s.IsActive)
            .ToListAsync();
    }
}
