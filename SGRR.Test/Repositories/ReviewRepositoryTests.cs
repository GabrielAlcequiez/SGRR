using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;

namespace SGRR.Test.Repositories;

public class ReviewRepositoryTests : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _context;
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new AppDbContext(_options);
        _repository = new ReviewRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReviewExists_ReturnsReview()
    {
        var review = CreateReview();
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(review.Id);

        Assert.NotNull(found);
        Assert.Equal(review.Rating, found.Rating);
        Assert.Equal(review.Comment, found.Comment);
        Assert.Equal(review.CustomerName, found.CustomerName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenReviewDoesNotExist_ReturnsNull()
    {
        var found = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(found);
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_ReturnsOnlyReviewsOfThatRestaurant()
    {
        var restaurantId = Guid.NewGuid();
        var otherRestaurantId = Guid.NewGuid();
        var reviews = new[]
        {
            CreateReview(restaurantId), CreateReview(restaurantId), CreateReview(otherRestaurantId)
        };
        _context.Reviews.AddRange(reviews);
        await _context.SaveChangesAsync();

        var found = (await _repository.GetByRestaurantIdAsync(restaurantId)).ToArray();

        Assert.Equal(2, found.Length);
        Assert.All(found, review => Assert.Equal(restaurantId, review.RestaurantId));
    }

    [Fact]
    public async Task GetByRestaurantIdAsync_WhenNoReviews_ReturnsEmpty()
    {
        var found = await _repository.GetByRestaurantIdAsync(Guid.NewGuid());

        Assert.Empty(found);
    }

    [Fact]
    public async Task AddAsync_PersistsReview()
    {
        var review = CreateReview();

        await _repository.AddAsync(review);

        using var nextRequest = new AppDbContext(_options);
        var found = await nextRequest.Reviews.AsNoTracking().FirstOrDefaultAsync(item => item.Id == review.Id);
        Assert.NotNull(found);
        Assert.Equal(review.Comment, found.Comment);
    }

    [Fact]
    public async Task AddAsync_WhenReviewIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesReviewFields()
    {
        var review = CreateReview();
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        review.Rating = 1;
        review.Comment = "Comentario actualizado";

        await _repository.UpdateAsync(review);

        using var nextRequest = new AppDbContext(_options);
        var found = await nextRequest.Reviews.AsNoTracking().FirstOrDefaultAsync(item => item.Id == review.Id);
        Assert.NotNull(found);
        Assert.Equal(1, found.Rating);
        Assert.Equal("Comentario actualizado", found.Comment);
    }

    [Fact]
    public async Task UpdateAsync_WhenReviewIsNull_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingReview()
    {
        var review = CreateReview();
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(review.Id);

        Assert.Null(await _repository.GetByIdAsync(review.Id));
    }

    [Fact]
    public async Task DeleteAsync_WhenReviewDoesNotExist_DoesNotThrow()
    {
        await _repository.DeleteAsync(Guid.NewGuid());
    }

    private static Review CreateReview(Guid? restaurantId = null) => new()
    {
        RestaurantId = restaurantId ?? Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        CustomerName = "Cliente",
        Rating = 5,
        Comment = "Excelente servicio"
    };

    public void Dispose() => _context.Dispose();
}
