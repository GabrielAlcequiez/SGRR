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

    // Acción para el Panel de Estadísticas y Analítica (RF-13)
    [HttpGet]
    public async Task<IActionResult> Dashboard(TimeRangeFilter filter = TimeRangeFilter.AllTime)
    {
        if (CurrentOwner.Restaurant == null)
        {
            return View(new OwnerDashboardDto());
        }

        var metrics = await _metricsService.GetDashboardMetricsAsync(CurrentOwner.Restaurant.Id, filter);
        return View(metrics);
    }

    // Acción para la gestión del perfil de "Mi Restaurante"
    [HttpGet]
    public IActionResult MiRestaurante()
    {
        return View(CurrentOwner.Restaurant);
    }
}