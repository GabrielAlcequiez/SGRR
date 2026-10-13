using SGRR.Data.Entities;
using SGRR.Data.Models;

namespace SGRR.Data.Interfaces
{
    public interface IRestaurantRepository
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<IEnumerable<Restaurant>> SearchAsync(RestaurantSearchFilterDto filters);
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<bool> ExistsByNameAndLocationAsync(string name, string location);
        Task AddAsync(Restaurant restaurant);
        Task UpdateAsync(Restaurant restaurant);
        Task DeleteAsync(Guid id);
        
    }
}
