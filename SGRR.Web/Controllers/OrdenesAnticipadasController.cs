using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Ordenes;

namespace SGRR.Web.Controllers;

public class OrdenesAnticipadasController : Controller
{
    private const int LeadMinutes = 30;

    private readonly IOrdenAnticipadaService _service;
    private readonly IRestaurantRepository _restaurants;
    private readonly IMenuRepository _menus;
    private readonly IUserRepository _users;
    private User _client = null!;

    public OrdenesAnticipadasController(
        IOrdenAnticipadaService service,
        IRestaurantRepository restaurants,
        IMenuRepository menus,
        IUserRepository users)
    {
        _service = service;
        _restaurants = restaurants;
        _menus = menus;
        _users = users;
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
    public async Task<IActionResult> Index(Guid restaurantId)
    {
        if (restaurantId == Guid.Empty)
            return RedirectToAction("Index", "Client");

        return await FormularioAsync(new CreateOrdenAnticipadaDto
        {
            RestaurantId = restaurantId,
            FechaProgramada = DateTime.Today,
            HoraProgramada = HoraSugerida(DateTime.Today)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CreateOrdenAnticipadaDto dto)
    {
        if (!ModelState.IsValid)
            return await FormularioAsync(dto);

        var code = await _service.CrearAsync(dto, _client.Id);
        if (string.IsNullOrEmpty(code))
            return await FormularioAsync(dto);

        // Redirección para que actualizar la pantalla no reenvíe la orden.
        TempData["OrdenCreadaCode"] = code;
        return RedirectToAction(nameof(MisOrdenes));
    }

    [HttpGet]
    public async Task<IActionResult> MisOrdenes()
    {
        ViewBag.OrdenCreadaCode = TempData["OrdenCreadaCode"] as string;
        return View(await _service.GetByClientIdAsync(_client.Id));
    }

    private async Task<IActionResult> FormularioAsync(CreateOrdenAnticipadaDto dto)
    {
        var restaurant = await _restaurants.GetByIdAsync(dto.RestaurantId);
        if (restaurant is null)
            return NotFound();
        dto.RestaurantName = restaurant.Name;

        var menu = await _menus.GetByRestaurantIdAsync(dto.RestaurantId);
        ViewBag.CategorizedMenu = menu
            .GroupBy(item => item.Category)
            .ToDictionary(group => group.Key, group => group.ToList());

        var schedules = await _menus.GetSchedulesByRestaurantIdAsync(dto.RestaurantId);
        var daySchedule = schedules.FirstOrDefault(schedule => schedule.DayOfWeek == dto.FechaProgramada.DayOfWeek);
        ViewBag.ScheduleText = daySchedule is null
            ? null
            : $"Horario de atención: {daySchedule.OpeningTime.ToString(@"hh\:mm")} - {daySchedule.ClosingTime.ToString(@"hh\:mm")}";

        ViewBag.HoraMinima = HoraSugerida(dto.FechaProgramada).ToString(@"hh\:mm");
        return View("Index", dto);
    }

    // Hora mínima de selección: para hoy, 30 minutos después de la hora actual; para otros días, sin restricción.
    private static TimeSpan HoraSugerida(DateTime fecha)
    {
        var ahora = DateTime.Now;
        if (fecha.Date != ahora.Date)
            return TimeSpan.Zero;

        var sugerida = ahora.AddMinutes(LeadMinutes);
        return new TimeSpan(sugerida.Hour, sugerida.Minute, 0);
    }
}
