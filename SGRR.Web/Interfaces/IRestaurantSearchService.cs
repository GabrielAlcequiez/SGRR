using SGRR.Data.Entities;
using SGRR.Data.Models;

namespace SGRR.Web.Interfaces;

public interface IRestaurantSearchService
{
    Task<IEnumerable<Restaurant>> SearchAsync(RestaurantSearchFilterDto filters);
}
