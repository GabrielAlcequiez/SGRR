using SGRR.Data.Common.Enums;

namespace SGRR.Web.Models.Reports;

// ViewModel de la pantalla de reportes admin: parámetros del formulario + resultados previos
public class AdminReportsViewModel
{
    // Filtro de tiempo del reporte (igual criterio que el panel del propietario: "Completed" o "Completada")
    public AdminReportType SelectedType { get; set; } = AdminReportType.RestaurantPerformance;

    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime EndDate { get; set; } = DateTime.Today;

    // Filas del reporte seleccionado para la tabla previa a la descarga
    public List<RestaurantPerformanceRow> PerformanceRows { get; set; } = new();
    public List<GlobalSalesRow> SalesRows { get; set; } = new();

    // Indica si hay datos que mostrar/exportar según el tipo seleccionado
    public bool HasResults
    {
        get
        {
            return SelectedType == AdminReportType.RestaurantPerformance
                ? PerformanceRows.Any()
                : SalesRows.Any();
        }
    }
}
