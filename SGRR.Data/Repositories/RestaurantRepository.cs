using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly AppDbContext _context;

        public RestaurantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync()
        {
            return await _context.Restaurants.ToListAsync();
        }

        public async Task<Restaurant?> GetByIdAsync(Guid id)
        {
            return await _context.Restaurants.FindAsync(id);
        }

        public Task<bool> ExistsByNameAndLocationAsync(string name, string location)
        {
            string normalizedName = name.Trim().ToLowerInvariant();
            string normalizedLocation = location.Trim().ToLowerInvariant();

            return _context.Restaurants.AnyAsync(restaurant =>
                restaurant.Name.ToLower() == normalizedName &&
                restaurant.Location.ToLower() == normalizedLocation);
        }

        public async Task AddAsync(Restaurant restaurant)
        {
            await _context.Restaurants.AddAsync(restaurant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Restaurant restaurant)
        {
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant != null)
            {
                var owners = await _context.Users.Include(user => user.Restaurant)
                    .Where(user => user.Restaurant != null && user.Restaurant.Id == id)
                    .ToListAsync();
                foreach (var owner in owners)
                    owner.ClearRestaurant();

                var dishes = await _context.Dishes.Where(dish => dish.RestaurantId == id).ToListAsync();
                _context.Dishes.RemoveRange(dishes);
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
