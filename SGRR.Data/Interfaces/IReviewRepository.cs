using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id);
    Task<IEnumerable<Review>> GetByRestaurantIdAsync(Guid restaurantId, ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Guid id);
}
