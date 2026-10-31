using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Moq;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Services;

namespace SGRR.Test.Services
{
    public class ReviewServiceTests
    {
        [Fact]
        public async Task GetByRestaurantAsync_NonExistingRestaurant_ThrowsKeyNotFoundException()
        {
            // Arrange

            var restaurantId = Guid.NewGuid();
            var repoMock = new Mock<IRestaurantRepository>();

            repoMock
                .Setup(repo => repo.GetByIdAsync(restaurantId))
                .ReturnsAsync((Restaurant)null!);

            var service = new ReviewService(
                Mock.Of<IReviewRepository>(),
                repoMock.Object,
                Mock.Of<IUserRepository>(),
                Mock.Of<IRestaurantReviewLock>(),
                Mock.Of<ISystemActivityLogRepository>()
            );

            // Act

            Func<Task> act = () =>
             service.GetByRestaurantAsync(restaurantId);

            // Assert

            await Assert.ThrowsAsync<KeyNotFoundException>(act);
        }

        [Fact]
        public async Task GetByIdForCustomerAsync_NonExistingReview_ThrowsKeyNotFoundException()
        {
            // Arrange

            var reviewId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var repoMock = new Mock<IReviewRepository>();

            repoMock
                .Setup(repo => repo.GetByIdAsync(reviewId))
                .ReturnsAsync((Review?)null);

            var service = new ReviewService(
                repoMock.Object,
                Mock.Of<IRestaurantRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<IRestaurantReviewLock>(),
                Mock.Of<ISystemActivityLogRepository>()
            );

            // Act

            Func<Task> act = () => service.GetByIdForCustomerAsync(reviewId, customerId);

            //Assert

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
            Assert.Equal("Reseña no encontrada.", ex.Message);
        }

        [Fact]
        public async Task GetByIdForCustomerAsync_ReviewOwnedByAnotherCustomer_ThrowsUnauthorizedAccessException()
        {
            // Arrange

            var reviewId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var requestCustomerId = Guid.NewGuid();

            var repoMock = new Mock<IReviewRepository>();

            var review = new Review
            {
                Id = reviewId,
                CustomerId = ownerId
            };

            repoMock
                .Setup(repo => repo.GetByIdAsync(reviewId))
                .ReturnsAsync(review);

            var service = new ReviewService(
                repoMock.Object,
                Mock.Of<IRestaurantRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<IRestaurantReviewLock>(),
                Mock.Of<ISystemActivityLogRepository>()
            );

            // Act

            Func<Task> act = () => service.GetByIdForCustomerAsync(reviewId, requestCustomerId);

            // Assert

            await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public async Task CreateAsync_InvalidRating_ThrowsArgumentOutOfRangeException(int rating)
        {
            // Arrange
            var input = new ReviewWriteDto
            {
                Rating = rating,
                Comment = "Excelente restaurante"
            };

            var service = new ReviewService(
                Mock.Of<IReviewRepository>(),
                Mock.Of<IRestaurantRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<IRestaurantReviewLock>(),
                Mock.Of<ISystemActivityLogRepository>()
            );

            // Act
            Func<Task> act = () => service.CreateAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                input
            );

            // Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(act);
        }

        [Theory]
[InlineData(new int[] { 5, 3, 1 }, 3.0)]
[InlineData(new int[] { 5, 4, 3, 2 }, 3.5)]
[InlineData(new int[] { 5, 5, 4 }, 4.7)]
[InlineData(new int[] { 1, 2, 2 }, 1.7)]
[InlineData(new int[] { 5, 4 }, 4.5)]
public async Task CreateAsync_MultipleRatings_CalculatesCorrectAverage(
    int[] ratings, double expectedAverage)
{
    // Arrange
    var restaurant = new Restaurant();
    var customer = new User(
    name: "Juan",
    lastName: "Pérez",
    email: "juan@test.com",
    password: "Test123!",
    role: Roles.Client
)
{
    Id = Guid.NewGuid()
};

    var reviews = ratings.Select(r => new Review
    {
        Rating = r
    }).ToList();

    var reviewMock = new Mock<IReviewRepository>();
    var restaurantMock = new Mock<IRestaurantRepository>();
    var userMock = new Mock<IUserRepository>();

    reviewMock
        .Setup(r => r.GetByRestaurantIdAsync(
            restaurant.Id, It.IsAny<ReviewSortOrder>()))
        .ReturnsAsync(reviews);

    restaurantMock
        .Setup(r => r.GetByIdAsync(restaurant.Id))
        .ReturnsAsync(restaurant);

    userMock
        .Setup(r => r.GetById(customer.Id))
        .Returns(customer);

    var service = new ReviewService(
        reviewMock.Object,
        restaurantMock.Object,
        userMock.Object,
        Mock.Of<IRestaurantReviewLock>(),
        Mock.Of<ISystemActivityLogRepository>()
    );

    // Act
    await service.CreateAsync(
        restaurant.Id,
        customer.Id,
        new ReviewWriteDto { Rating = 5, Comment = "Prueba" });

    // Assert
    Assert.Equal((decimal)expectedAverage, restaurant.Rating);
    Assert.Equal(ratings.Length, restaurant.ReviewCount);
}


    }
}