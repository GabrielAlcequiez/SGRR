using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories
{
    public class DishRepository : IDishRepository
    {
        private readonly AppDbContext _context;

        public DishRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dish>> GetAllAsync()
        {
            return await _context.Dishes.ToListAsync();
        }

        public async Task<IEnumerable<Dish>> GetByRestaurantIdAsync(Guid restaurantId)
        {
            return await _context.Dishes
                .Where(d => d.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<Dish?> GetByIdAsync(Guid id)
        {
            return await _context.Dishes.FindAsync(id);
        }

        public async Task AddAsync(Dish dish)
        {
            await _context.Dishes.AddAsync(dish);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Dish dish)
        {
            _context.Dishes.Update(dish);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {
                _context.Dishes.Remove(dish);
                await _context.SaveChangesAsync();
            }
        }
    }
}
