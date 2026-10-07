using SGRR.Data.Entities;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<Restaurant> CreateAsync(CreateRestaurantDto dto);
        Task UpdateAsync(UpdateRestaurantDto dto);
        Task DeleteAsync(Guid id);
    }
}
