using System.Collections.Concurrent;

namespace SGRR.Web.Services;

public class RestaurantReviewLock : IRestaurantReviewLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<IDisposable> AcquireAsync(Guid restaurantId)
    {
        var semaphore = _locks.GetOrAdd(restaurantId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        return new SemaphoreReleaser(semaphore);
    }

    private sealed class SemaphoreReleaser(SemaphoreSlim semaphore) : IDisposable
    {
        private SemaphoreSlim? _semaphore = semaphore;

        public void Dispose()
        {
            Interlocked.Exchange(ref _semaphore, null)?.Release();
        }
    }
}
