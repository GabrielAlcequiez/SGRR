using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class ReviewService : IReviewService
{
    public const int MaximumCommentLength = 1000;
    private readonly IReviewRepository _reviewRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantReviewLock _reviewLock;
    private readonly ISystemActivityLogRepository _logRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IRestaurantReviewLock reviewLock,
        ISystemActivityLogRepository logRepository)
    {
        _reviewRepository = reviewRepository;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _reviewLock = reviewLock;
        _logRepository = logRepository;
    }

    public async Task<IEnumerable<ReviewDto>> GetByRestaurantAsync(
        Guid restaurantId,
        ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst)
    {
        using (await _reviewLock.AcquireAsync(restaurantId))
        {
            if (await _restaurantRepository.GetByIdAsync(restaurantId) is null)
                throw new KeyNotFoundException("Restaurante no encontrado.");

            var reviews = await _reviewRepository.GetByRestaurantIdAsync(restaurantId, sortOrder);
            return reviews.Select(ToDto).ToList();
        }
    }

    public async Task<ReviewDto> GetByIdForCustomerAsync(Guid reviewId, Guid customerId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException("Reseña no encontrada.");

        EnsureCustomerOwnsReview(review, customerId);
        return ToDto(review);
    }

    public async Task<ReviewDto> CreateAsync(Guid restaurantId, Guid customerId, ReviewWriteDto input)
    {
        ValidateInput(input);
        using (await _reviewLock.AcquireAsync(restaurantId))
        {
            var customer = GetActiveCustomer(customerId);
            var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId)
                ?? throw new KeyNotFoundException("Restaurante no encontrado.");

            var review = new Review
            {
                RestaurantId = restaurantId,
                CustomerId = customer.Id,
                CustomerName = $"{customer.Name} {customer.LastName}".Trim(),
                Rating = input.Rating,
                Comment = input.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            await RecalculateRestaurantRatingAsync(restaurant);
            var customerName = $"{customer.Name} {customer.LastName}".Trim();
            _logRepository.AddLog(
                LogSeverity.Info,
                $"El cliente '{(string.IsNullOrWhiteSpace(customerName) ? customer.Email : customerName)}' dejó una reseña de {review.Rating} estrellas para el restaurante '{restaurant.Name}'.");
            return ToDto(review);
        }
    }

    public async Task<ReviewDto> UpdateAsync(Guid reviewId, Guid customerId, ReviewWriteDto input)
    {
        ValidateInput(input);
        var original = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException("Reseña no encontrada.");

        using (await _reviewLock.AcquireAsync(original.RestaurantId))
        {
            var customer = GetActiveCustomer(customerId);
            var review = await _reviewRepository.GetByIdAsync(reviewId)
                ?? throw new KeyNotFoundException("Reseña no encontrada.");
            EnsureCustomerOwnsReview(review, customerId);

            review.Rating = input.Rating;
            review.Comment = input.Comment.Trim();
            await _reviewRepository.UpdateAsync(review);

            var restaurant = await _restaurantRepository.GetByIdAsync(review.RestaurantId)
                ?? throw new KeyNotFoundException("Restaurante no encontrado.");
            await RecalculateRestaurantRatingAsync(restaurant);
            var customerName = $"{customer.Name} {customer.LastName}".Trim();
            _logRepository.AddLog(
                LogSeverity.Info,
                $"El cliente '{(string.IsNullOrWhiteSpace(customerName) ? customer.Email : customerName)}' actualizó su reseña a {review.Rating} estrellas para el restaurante '{restaurant.Name}'.");
            return ToDto(review);
        }
    }

    public async Task DeleteAsync(Guid reviewId, Guid customerId)
    {
        var original = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new KeyNotFoundException("Reseña no encontrada.");

        using (await _reviewLock.AcquireAsync(original.RestaurantId))
        {
            var customer = GetActiveCustomer(customerId);
            var review = await _reviewRepository.GetByIdAsync(reviewId)
                ?? throw new KeyNotFoundException("Reseña no encontrada.");
            EnsureCustomerOwnsReview(review, customerId);

            await _reviewRepository.DeleteAsync(reviewId);
            var restaurant = await _restaurantRepository.GetByIdAsync(review.RestaurantId)
                ?? throw new KeyNotFoundException("Restaurante no encontrado.");
            await RecalculateRestaurantRatingAsync(restaurant);
            var customerName = $"{customer.Name} {customer.LastName}".Trim();
            _logRepository.AddLog(
                LogSeverity.Info,
                $"El cliente '{(string.IsNullOrWhiteSpace(customerName) ? customer.Email : customerName)}' eliminó su reseña para el restaurante '{restaurant.Name}'.");
        }
    }

    private async Task RecalculateRestaurantRatingAsync(Restaurant restaurant)
    {
        var reviews = (await _reviewRepository.GetByRestaurantIdAsync(restaurant.Id)).ToList();
        restaurant.ReviewCount = reviews.Count;
        restaurant.Rating = reviews.Count == 0
            ? null
            : Math.Round(reviews.Sum(review => (decimal)review.Rating) / reviews.Count, 1, MidpointRounding.AwayFromZero);

        await _restaurantRepository.UpdateAsync(restaurant);
    }

    private User GetActiveCustomer(Guid customerId)
    {
        var customer = _userRepository.GetById(customerId);
        if (customer is null || customer.Role != Roles.Client || !customer.IsActive)
            throw new UnauthorizedAccessException("Se requiere una cuenta de cliente activa para gestionar reseñas.");

        return customer;
    }

    private static void EnsureCustomerOwnsReview(Review review, Guid customerId)
    {
        if (review.CustomerId != customerId)
            throw new UnauthorizedAccessException("Solo puedes modificar o eliminar tus propias reseñas.");
    }

    private static void ValidateInput(ReviewWriteDto input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(input.Rating), "La puntuación debe estar entre 1 y 5 estrellas.");
        if (string.IsNullOrWhiteSpace(input.Comment))
            throw new ArgumentException("El comentario es obligatorio.", nameof(input.Comment));
        if (input.Comment.Trim().Length > MaximumCommentLength)
            throw new ArgumentException($"El comentario no puede superar los {MaximumCommentLength} caracteres.", nameof(input.Comment));
    }

    private static ReviewDto ToDto(Review review) => new()
    {
        Id = review.Id,
        RestaurantId = review.RestaurantId,
        CustomerId = review.CustomerId,
        CustomerName = review.CustomerName,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}
