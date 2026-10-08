using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;

namespace SGRR.Web.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;

    public MenuService(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<RestaurantMenuViewModel> GetMenuAndSchedulesAsync(Guid restaurantId, DateTime selectedDate)
    {
        var model = new RestaurantMenuViewModel { RestaurantId = restaurantId };

        // 1. Consulta de platos y organización por categorías
        var items = await _menuRepository.GetByRestaurantIdAsync(restaurantId);
        if (items == null || !items.Any())
        {
            model.MenuErrorMessage = "Este restaurante no tiene platillos disponibles en su menú en este momento.";
        }
        else
        {
            model.CategorizedMenu = items
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // 2. Consulta y filtrado de horarios de atención
        var schedules = await _menuRepository.GetSchedulesByRestaurantIdAsync(restaurantId);
        var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == selectedDate.DayOfWeek);

        if (daySchedule == null)
        {
            model.ScheduleErrorMessage = "El restaurante se encuentra fuera de servicio en la fecha seleccionada.";
        }
        else
        {
            var current = daySchedule.OpeningTime;
            while (current < daySchedule.ClosingTime)
            {
                model.AvailableHours.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(30));
            }
        }

        return model;
    }
}
