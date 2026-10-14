using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class ReviewRepository : IReviewRepository
{
    // EF In-Memory shares its store between request-scoped DbContexts. Serialize store access
    // so concurrent requests cannot interleave changes to that shared store.
    private static readonly SemaphoreSlim StoreGate = new(1, 1);
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        await StoreGate.WaitAsync();
        try
        {
            return await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(review => review.Id == id);
        }
        finally
        {
            StoreGate.Release();
        }
    }

    public async Task<IEnumerable<Review>> GetByRestaurantIdAsync(
        Guid restaurantId,
        ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst)
    {
        await StoreGate.WaitAsync();
        try
        {
            var query = _context.Reviews.AsNoTracking().Where(review => review.RestaurantId == restaurantId);
            return sortOrder == ReviewSortOrder.OldestFirst
                ? await query.OrderBy(review => review.CreatedAt).ThenBy(review => review.Id).ToListAsync()
                : await query.OrderByDescending(review => review.CreatedAt).ThenByDescending(review => review.Id).ToListAsync();
        }
        finally
        {
            StoreGate.Release();
        }
    }

    public async Task AddAsync(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        await StoreGate.WaitAsync();
        try
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }
        finally
        {
            StoreGate.Release();
        }
    }

    public async Task UpdateAsync(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        await StoreGate.WaitAsync();
        try
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }
        finally
        {
            StoreGate.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await StoreGate.WaitAsync();
        try
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(item => item.Id == id);
            if (review is null)
                return;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
        finally
        {
            StoreGate.Release();
        }
    }
}
