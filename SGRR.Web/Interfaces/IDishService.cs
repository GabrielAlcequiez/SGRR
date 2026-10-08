using SGRR.Data.Entities;
using SGRR.Web.Models.Dish;

namespace SGRR.Web.Interfaces
{
    public interface IDishService
    {
        Task<IEnumerable<Dish>> GetByRestaurantIdAsync(User requestor);
        Task<Dish?> GetByIdAsync(Guid id, User requestor);
        Task<Dish> CreateAsync(CreateDishDto dto, User requestor);
        Task UpdateAsync(UpdateDishDto dto, User requestor);
        Task DeleteAsync(Guid id, User requestor);
    }
}
