using System.ComponentModel.DataAnnotations;
using System.Globalization;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Services;

public class ReservationService : IReservationService
{
    // Simulación: 30 personas por bloque de 30 minutos entre todas las áreas.
    private const int MaxCapacityPerSlot = 30;
    private static readonly SemaphoreSlim ReservationLock = new(1, 1);
    public static readonly string[] Locations = ["Salón Principal", "Terraza", "VIP", "Barra"];
    private readonly IReservationRepository _reservations;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menus;
    private readonly IUserRepository _users;

    public ReservationService(IReservationRepository reservations, IRestaurantRepository restaurants,
        IMenuRepository menus, IUserRepository users)
    {
        _reservations = reservations;
        _restaurants = restaurants;
        _menus = menus;
        _users = users;
    }

    public async Task<bool> ValidateAvailabilityAsync(Guid restaurantId, DateTime date, string timeSlot, int peopleCount)
    {
        if (peopleCount is < 1 or > 20 || date.Date < DateTime.Today ||
            !TimeSpan.TryParseExact(timeSlot, @"hh\:mm", CultureInfo.InvariantCulture, out var time) ||
            date.Date.Add(time) <= DateTime.Now || await _restaurants.GetByIdAsync(restaurantId) is null)
            return false;
        var schedules = await _menus.GetSchedulesByRestaurantIdAsync(restaurantId);
        var validSlot = schedules.Any(s => s.IsActive && s.DayOfWeek == date.DayOfWeek &&
            time >= s.OpeningTime && time < s.ClosingTime &&
            (time - s.OpeningTime).Ticks % TimeSpan.FromMinutes(30).Ticks == 0);
        return validSlot && await _reservations.GetTotalReservedCapacityAsync(restaurantId, date, timeSlot)
            + peopleCount <= MaxCapacityPerSlot;
    }

    public async Task<string> RegisterReservationAsync(CreateReservationViewModel model, Guid clientId)
    {
        var client = _users.GetById(clientId);
        if (client is null || client.Role != Roles.Client || !client.IsActive ||
            !Validator.TryValidateObject(model, new ValidationContext(model), null, true) ||
            !Locations.Contains(model.TableLocation))
            return string.Empty;
        // Serializa comprobación y guardado para esta aplicación en memoria.
        await ReservationLock.WaitAsync();
        try
        {
            if (!await ValidateAvailabilityAsync(model.RestaurantId, model.Date, model.TimeSlot, model.PeopleCount) ||
                await _reservations.HasDuplicateReservationAsync(clientId, model.Date, model.TimeSlot))
                return string.Empty;
            var id = Guid.NewGuid();
            var reservation = new Reservation
            {
                Id = id, ClientId = clientId, RestaurantId = model.RestaurantId,
                Date = model.Date.Date, TimeSlot = model.TimeSlot, PeopleCount = model.PeopleCount,
                TableLocation = model.TableLocation, ReservationCode = $"RES-{id:N}", Status = "Active"
            };
            return await _reservations.CreateAsync(reservation) ? reservation.ReservationCode : string.Empty;
        }
        finally { ReservationLock.Release(); }
    }

    public async Task<bool> CancelReservationAsync(Guid reservationId, Guid clientId)
    {
        var client = _users.GetById(clientId);
        if (client is null || client.Role != Roles.Client || !client.IsActive)
            return false;
        await ReservationLock.WaitAsync();
        try
        {
            var reservation = await _reservations.GetByIdAsync(reservationId);
            if (reservation is null || reservation.ClientId != clientId || reservation.Status != "Active")
                return false;
            reservation.Status = "Canceled";
            return await _reservations.UpdateAsync(reservation);
        }
        finally { ReservationLock.Release(); }
    }
}
