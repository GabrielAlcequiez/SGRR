using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Controllers;

public class ReviewController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;

    public ReviewController(
        IReviewService reviewService,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository)
    {
        _reviewService = reviewService;
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid restaurantId, ReviewSortOrder sortOrder = ReviewSortOrder.NewestFirst)
    {
        if (!TryGetCurrentClient(out var customer, out var failure))
            return failure!;

        if (!Enum.IsDefined(sortOrder))
            sortOrder = ReviewSortOrder.NewestFirst;

        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId);
        if (restaurant is null)
            return NotFound();

        return View(await BuildPageAsync(restaurant, customer!, sortOrder));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewPageViewModel model)
    {
        if (!TryGetCurrentClient(out var customer, out var failure))
            return failure!;

        var restaurant = await _restaurantRepository.GetByIdAsync(model.RestaurantId);
        if (restaurant is null)
            return NotFound();

        if (!ModelState.IsValid)
            return View("Index", await BuildPageAsync(restaurant, customer!, model.SortOrder, model.Rating, model.Comment));

        try
        {
            await _reviewService.CreateAsync(model.RestaurantId, customer!.Id, new ReviewWriteDto
            {
                Rating = model.Rating,
                Comment = model.Comment
            });
            TempData["SuccessMessage"] = "Tu reseña se publicó correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = model.RestaurantId });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ModelState.AddModelError(nameof(model.Rating), ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(model.Comment), ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return View("Index", await BuildPageAsync(restaurant, customer!, model.SortOrder, model.Rating, model.Comment));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid reviewId)
    {
        if (!TryGetCurrentClient(out var customer, out var failure))
            return failure!;

        try
        {
            var review = await _reviewService.GetByIdForCustomerAsync(reviewId, customer!.Id);
            var restaurant = await _restaurantRepository.GetByIdAsync(review.RestaurantId);
            if (restaurant is null)
                return NotFound();

            return View(new EditReviewViewModel
            {
                ReviewId = review.Id,
                RestaurantId = restaurant.Id,
                RestaurantName = restaurant.Name,
                Rating = review.Rating,
                Comment = review.Comment
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid reviewId, EditReviewViewModel model)
    {
        if (!TryGetCurrentClient(out var customer, out var failure))
            return failure!;

        ReviewDto review;
        try
        {
            review = await _reviewService.GetByIdForCustomerAsync(reviewId, customer!.Id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var restaurant = await _restaurantRepository.GetByIdAsync(review.RestaurantId);
        if (restaurant is null)
            return NotFound();

        model.ReviewId = reviewId;
        model.RestaurantId = restaurant.Id;
        model.RestaurantName = restaurant.Name;
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _reviewService.UpdateAsync(reviewId, customer!.Id, new ReviewWriteDto
            {
                Rating = model.Rating,
                Comment = model.Comment
            });
            TempData["SuccessMessage"] = "Tu reseña se actualizó correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = restaurant.Id });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ModelState.AddModelError(nameof(model.Rating), ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(model.Comment), ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid reviewId)
    {
        if (!TryGetCurrentClient(out var customer, out var failure))
            return failure!;

        try
        {
            var review = await _reviewService.GetByIdForCustomerAsync(reviewId, customer!.Id);
            await _reviewService.DeleteAsync(reviewId, customer.Id);
            TempData["SuccessMessage"] = "Tu reseña se eliminó correctamente.";
            return RedirectToAction(nameof(Index), new { restaurantId = review.RestaurantId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
    }

    private async Task<ReviewPageViewModel> BuildPageAsync(
        Restaurant restaurant,
        User customer,
        ReviewSortOrder sortOrder,
        int rating = 0,
        string comment = "")
    {
        var reviews = await _reviewService.GetByRestaurantAsync(restaurant.Id, sortOrder);
        var reviewList = reviews.ToList();
        return new ReviewPageViewModel
        {
            RestaurantId = restaurant.Id,
            RestaurantName = restaurant.Name,
            AverageRating = reviewList.Count == 0
                ? null
                : Math.Round(reviewList.Sum(review => (decimal)review.Rating) / reviewList.Count, 1, MidpointRounding.AwayFromZero),
            ReviewCount = reviewList.Count,
            CurrentCustomerId = customer.Id,
            SortOrder = sortOrder,
            Rating = rating,
            Comment = comment,
            Reviews = reviewList
        };
    }

    private bool TryGetCurrentClient(out User? customer, out IActionResult? failure)
    {
        customer = null;
        failure = null;
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            failure = RedirectToAction("Login", "Auth");
            return false;
        }

        customer = _userRepository.GetById(userId);
        if (customer is null)
        {
            failure = RedirectToAction("Login", "Auth");
            return false;
        }

        if (customer.Role != Roles.Client || !customer.IsActive)
        {
            failure = StatusCode(StatusCodes.Status403Forbidden);
            return false;
        }

        return true;
    }
}
