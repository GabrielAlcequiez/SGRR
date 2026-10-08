using SGRR.Data.Entities;
using SGRR.Web.Models.Restaurant;

namespace SGRR.Web.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<Restaurant>> GetAllAsync();
        Task<IEnumerable<Restaurant>> SearchAsync(string? search);
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<Restaurant> CreateAsync(CreateRestaurantDto dto, User requestor);
        Task UpdateAsync(UpdateRestaurantDto dto, User requestor);
        Task DeleteAsync(Guid id, User requestor);
    }
}
