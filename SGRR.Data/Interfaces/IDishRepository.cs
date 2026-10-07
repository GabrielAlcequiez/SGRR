using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces
{
    public interface IDishRepository
    {
        Task<IEnumerable<Dish>> GetAllAsync();
        Task<IEnumerable<Dish>> GetByRestaurantIdAsync(Guid restaurantId);
        Task<Dish?> GetByIdAsync(Guid id);
        Task AddAsync(Dish dish);
        Task UpdateAsync(Dish dish);
        Task DeleteAsync(Guid id);
    }
}
