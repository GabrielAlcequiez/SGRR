using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;

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

        public async Task<IEnumerable<Restaurant>> SearchAsync(RestaurantSearchFilterDto filters)
        {
            ArgumentNullException.ThrowIfNull(filters);
            var query = _context.Restaurants.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filters.Specialty))
            {
                var specialty = filters.Specialty.Trim().ToLowerInvariant();
                query = query.Where(restaurant => restaurant.Specialty.ToLower().Contains(specialty));
            }

            if (!string.IsNullOrWhiteSpace(filters.Location))
            {
                var location = filters.Location.Trim().ToLowerInvariant();
                query = query.Where(restaurant => restaurant.Location.ToLower().Contains(location));
            }

            if (filters.MinimumRating.HasValue)
                query = query.Where(restaurant => restaurant.Rating >= filters.MinimumRating.Value);

            return await query.ToListAsync();
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
            if (_context.Entry(restaurant).State == EntityState.Detached)
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

                var menuItems = await _context.MenuItems.Where(item => item.RestaurantId == id).ToListAsync();
                _context.MenuItems.RemoveRange(menuItems);
                var schedules = await _context.RestaurantSchedules.Where(schedule => schedule.RestaurantId == id).ToListAsync();
                _context.RestaurantSchedules.RemoveRange(schedules);
                var reviews = await _context.Reviews.Where(review => review.RestaurantId == id).ToListAsync();
                _context.Reviews.RemoveRange(reviews);
                _context.Restaurants.Remove(restaurant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
