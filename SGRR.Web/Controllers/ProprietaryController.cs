using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Owner;

namespace SGRR.Web.Controllers;

public class ProprietaryController : OwnerController
{
    private readonly IOwnerMetricsService _metricsService;

    public ProprietaryController(
        IUserRepository userRepository,
        IOwnerMetricsService metricsService) : base(userRepository)
    {
        _metricsService = metricsService;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(TimeRangeFilter filter = TimeRangeFilter.AllTime)
    {
        // Validar que el propietario tenga un restaurante asociado a su cuenta
        if (CurrentOwner.Restaurant == null)
        {
            return View(new OwnerDashboardDto());
        }

        // Obtener las métricas usando el ID del restaurante del propietario actual
        var metrics = await _metricsService.GetDashboardMetricsAsync(CurrentOwner.Restaurant.Id, filter);
        
        return View(metrics);
    }
}