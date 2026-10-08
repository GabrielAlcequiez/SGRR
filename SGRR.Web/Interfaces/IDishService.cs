using SGRR.Data.Entities;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Interfaces
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetByRestaurantIdAsync(Guid restaurantId);
        Task<Dish?> GetByIdAsync(Guid id);
        Task<Dish> CreateAsync(CreateDishDto dto);
        Task UpdateAsync(UpdateDishDto dto);
        Task DeleteAsync(Guid id);
    }
}
