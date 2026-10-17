using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class RestaurantSearchService : IRestaurantSearchService
{
    private const string RatingRangeError = "La evaluación mínima debe estar entre 1 y 5.";
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantSearchService(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public Task<IEnumerable<Restaurant>> SearchAsync(RestaurantSearchFilterDto filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        if (filters.MinimumRating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(filters.MinimumRating), filters.MinimumRating, RatingRangeError);

        return _restaurantRepository.SearchAsync(filters);
    }
}
