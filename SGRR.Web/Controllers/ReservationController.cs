using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Controllers;

public class ReservationController : Controller
{
    private readonly IReservationService _service;
    private readonly IReservationRepository _reservations;
    private readonly IRestaurantRepository _restaurants;
    private readonly IUserRepository _users;
    private readonly IMenuService _menus;
    private User _client = null!;

    public ReservationController(IReservationService service, IReservationRepository reservations,
        IRestaurantRepository restaurants, IUserRepository users, IMenuService menus)
    {
        _service = service;
        _reservations = reservations;
        _restaurants = restaurants;
        _users = users;
        _menus = menus;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var id) || _users.GetById(id) is not { } user)
            context.Result = RedirectToAction("Login", "Auth");
        else if (user.Role != Roles.Client || !user.IsActive)
            context.Result = StatusCode(403);
        else
            _client = user;
        base.OnActionExecuting(context);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewBag.Restaurants = (await _restaurants.GetAllAsync()).ToDictionary(r => r.Id, r => r.Name);
        return View((await _reservations.GetByClientIdAsync(_client.Id)).OrderByDescending(r => r.Date).ToList());
    }

    [HttpGet]
    public Task<IActionResult> Create(Guid restaurantId, DateTime? date, string? timeSlot) =>
        ReservationForm(new CreateReservationViewModel
        {
            RestaurantId = restaurantId, Date = date ?? DateTime.Today, TimeSlot = timeSlot ?? ""
        });

    private async Task<IActionResult> ReservationForm(CreateReservationViewModel model)
    {
        var restaurant = await _restaurants.GetByIdAsync(model.RestaurantId);
        if (restaurant is null) return NotFound();
        model.RestaurantName = restaurant.Name;
        var menu = await _menus.GetMenuAndSchedulesAsync(model.RestaurantId, model.Date);
        var hours = new List<string>();
        foreach (var hour in menu.AvailableHours)
            if (await _service.ValidateAvailabilityAsync(model.RestaurantId, model.Date, hour, Math.Max(1, model.PeopleCount)))
                hours.Add(hour);
        ViewBag.AvailableHours = hours;
        if (!hours.Contains(model.TimeSlot))
        {
            model.TimeSlot = "";
            ModelState.Remove(nameof(model.TimeSlot));
        }
        return View("Create", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshHours(CreateReservationViewModel model)
    {
        // Se puede consultar antes de completar la ubicación y el horario.
        ModelState.Remove(nameof(model.TimeSlot));
        ModelState.Remove(nameof(model.TableLocation));
        ViewBag.HoursUpdated = true;
        return await ReservationForm(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmSummary(CreateReservationViewModel model)
    {
        var restaurant = await _restaurants.GetByIdAsync(model.RestaurantId);
        if (restaurant is null) return NotFound();
        model.RestaurantName = restaurant.Name;
        if (!ModelState.IsValid) return await ReservationForm(model);
        if (!await _service.ValidateAvailabilityAsync(model.RestaurantId, model.Date, model.TimeSlot, model.PeopleCount))
        {
            model.ErrorMessage = "No hay disponibilidad para la fecha, horario y cantidad de personas seleccionados.";
            return await ReservationForm(model);
        }
        return View("Summary", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitConfirmation(CreateReservationViewModel model)
    {
        if (!ModelState.IsValid) return await ReservationForm(model);
        var code = await _service.RegisterReservationAsync(model, _client.Id);
        if (string.IsNullOrEmpty(code))
        {
            model.ErrorMessage = "No se pudo reservar: revise los datos, el cupo o si ya tiene una reserva en ese horario.";
            return await ReservationForm(model);
        }
        // Redirección para que actualizar la pantalla no reenvíe la confirmación.
        return RedirectToAction(nameof(Success), new { code });
    }

    [HttpGet]
    public async Task<IActionResult> Success(string code)
    {
        var reservation = (await _reservations.GetByClientIdAsync(_client.Id)).FirstOrDefault(r => r.ReservationCode == code);
        if (reservation is null) return NotFound();
        var restaurant = await _restaurants.GetByIdAsync(reservation.RestaurantId);
        return View(new ReservationDetailsViewModel
        {
            ReservationCode = reservation.ReservationCode, RestaurantName = restaurant?.Name ?? "Restaurante no disponible",
            Date = reservation.Date.ToString("dd/MM/yyyy"), TimeSlot = reservation.TimeSlot,
            PeopleCount = reservation.PeopleCount, TableLocation = reservation.TableLocation, Status = reservation.Status
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid reservationId)
    {
        TempData["ReservationMessage"] = await _service.CancelReservationAsync(reservationId, _client.Id)
            ? "Reserva cancelada correctamente." : "No se pudo cancelar la reserva.";
        return RedirectToAction(nameof(Index));
    }
}
