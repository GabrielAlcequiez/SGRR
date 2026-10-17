using SGRR.Web.Models.Reports;

namespace SGRR.Web.Interfaces;
public interface IReportExportService
{
    string GenerateCsv(string[] headers, IEnumerable<string[]> rows);

    byte[] ExportRestaurantPerformance(List<RestaurantPerformanceRow> rows);

    byte[] ExportGlobalSales(List<GlobalSalesRow> rows);
}
