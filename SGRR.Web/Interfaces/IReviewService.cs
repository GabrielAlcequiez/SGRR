using SGRR.Data.Common.Enums;
using SGRR.Data.Models;

namespace SGRR.Web.Interfaces;

public interface IReviewService
{
    Task<IEnumerable<ReviewDto>> GetByRestaurantAsync(Guid restaurantId, ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst);
    Task<ReviewDto> GetByIdForCustomerAsync(Guid reviewId, Guid customerId);
    Task<ReviewDto> CreateAsync(Guid restaurantId, Guid customerId, ReviewWriteDto review);
    Task<ReviewDto> UpdateAsync(Guid reviewId, Guid customerId, ReviewWriteDto review);
    Task DeleteAsync(Guid reviewId, Guid customerId);
}
