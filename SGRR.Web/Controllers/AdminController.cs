using Microsoft.AspNetCore.Mvc;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Reports;

namespace SGRR.Web.Controllers;

public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IUserRepository _userRepository;
    private readonly IAdminReportService _reportService;
    private readonly IReportExportService _reportExportService;

    // Inyectamos también IUserRepository para poder buscar al admin en la base de datos usando el ID de la sesión
    public AdminController(
        IAdminService adminService,
        IUserRepository userRepository,
        IAdminReportService reportService,
        IReportExportService reportExportService)
    {
        _adminService = adminService;
        _userRepository = userRepository;
        _reportService = reportService;
        _reportExportService = reportExportService;
    }

    // Método privado para obtener el usuario real de la sesión
    private User? GetCurrentAdminUser()
    {
        var roleStr = HttpContext.Session.GetString("UserRole");
        if (roleStr != Roles.Administrator.ToString())
        {
            return null; // No es admin
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (Guid.TryParse(userIdStr, out Guid userId))
        {
            return _userRepository.GetById(userId);
        }

        return null;
    }

    // GET: Admin/Dashboard
    public IActionResult Dashboard()
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            // Si no hay sesión o no es admin, lo pateamos al Login
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var users = _adminService.GetAllUsers(adminUser);
            return View(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login", "Auth");
        }
    }

    // GET: Admin/Reports — formulario parametrizado + tabla previa de resultados
    public async Task<IActionResult> Reports(int? reportType, DateTime? startDate, DateTime? endDate)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Valores por defecto: últimos 30 días hasta hoy
        var end = endDate?.Date ?? DateTime.Today;
        var start = startDate?.Date ?? end.AddDays(-30);

        // Si el rango viene invertido lo corregimos
        if (start > end)
        {
            (start, end) = (end, start);
        }

        // Solo aceptamos tipos válidos del enum; cualquier otra cosa vuelve al reporte por defecto
        var selectedType = reportType.HasValue
            && Enum.IsDefined(typeof(AdminReportType), reportType.Value)
            ? (AdminReportType)reportType.Value
            : AdminReportType.RestaurantPerformance;

        var model = new AdminReportsViewModel
        {
            SelectedType = selectedType,
            StartDate = start,
            EndDate = end
        };

        try
        {
            if (selectedType == AdminReportType.GlobalSalesBalance)
            {
                model.SalesRows = await _reportService.GetGlobalSalesAsync(start, end);
            }
            else
            {
                model.PerformanceRows = await _reportService.GetRestaurantPerformanceAsync(start, end);
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(model);
        }
    }

    // GET: Admin/ExportCsv — genera y descarga el CSV del reporte con los mismos parámetros
    public async Task<IActionResult> ExportCsv(int? reportType, DateTime? startDate, DateTime? endDate)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var end = endDate?.Date ?? DateTime.Today;
        var start = startDate?.Date ?? end.AddDays(-30);

        if (start > end)
        {
            (start, end) = (end, start);
        }

        var selectedType = reportType.HasValue
            && Enum.IsDefined(typeof(AdminReportType), reportType.Value)
            ? (AdminReportType)reportType.Value
            : AdminReportType.RestaurantPerformance;

        try
        {
            if (selectedType == AdminReportType.GlobalSalesBalance)
            {
                var salesRows = await _reportService.GetGlobalSalesAsync(start, end);
                var salesBytes = _reportExportService.ExportGlobalSales(salesRows);
                var salesFileName = $"reporte_balance_ventas_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
                return File(salesBytes, "text/csv; charset=utf-8", salesFileName);
            }

            var performanceRows = await _reportService.GetRestaurantPerformanceAsync(start, end);
            var performanceBytes = _reportExportService.ExportRestaurantPerformance(performanceRows);
            var performanceFileName = $"reporte_rendimiento_restaurantes_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
            return File(performanceBytes, "text/csv; charset=utf-8", performanceFileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Reports), new
            {
                reportType = (int)selectedType,
                startDate,
                endDate
            });
        }
    }

    // POST: Admin/ToggleStatus
    [HttpPost]
    public IActionResult ToggleStatus(Guid userId, bool isActive)
    {
        var adminUser = GetCurrentAdminUser();
        if (adminUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            _adminService.ToggleUserStatus(userId, isActive, adminUser);
            TempData["SuccessMessage"] = isActive ? "Usuario habilitado correctamente." : "Usuario bloqueado correctamente.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}