namespace SGRR.Web.Services;

public interface IRestaurantReviewLock
{
    Task<IDisposable> AcquireAsync(Guid restaurantId);
}
