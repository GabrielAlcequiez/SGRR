using System.Globalization;
using System.Text;
using SGRR.Web.Interfaces;
using SGRR.Web.Models.Reports;

namespace SGRR.Web.Services;

public class ReportExportService : IReportExportService
{
    // Encabezados fijos de cada reporte (mantienen el orden de las columnas exportadas)
    private static readonly string[] RestaurantPerformanceHeaders =
    [
        "Restaurante",
        "Especialidad",
        "Ubicacion",
        "Calificacion",
        "Cantidad de Resenias",
        "Pedidos Completados",
        "Ingresos Totales",
        "Ticket Promedio",
        "Reservas Confirmadas"
    ];

    private static readonly string[] GlobalSalesHeaders =
    [
        "Fecha",
        "Restaurante",
        "Cantidad de Pedidos",
        "Ingresos Totales"
    ];

    public string GenerateCsv(string[] headers, IEnumerable<string[]> rows)
    {
        var sb = new StringBuilder();

        // Línea de encabezados
        for (int i = 0; i < headers.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            sb.Append(EscapeField(headers[i]));
        }

        sb.AppendLine();

        // Líneas de datos
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(EscapeField(row[i]));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public byte[] ExportRestaurantPerformance(List<RestaurantPerformanceRow> rows)
    {
        var csvRows = rows.Select(row => new[]
        {
            row.RestaurantName,
            row.Specialty,
            row.Location,
            FormatDecimal(row.Rating),
            row.ReviewCount.ToString(CultureInfo.InvariantCulture),
            row.CompletedOrdersCount.ToString(CultureInfo.InvariantCulture),
            FormatDecimal(row.TotalRevenue),
            FormatDecimal(row.AverageTicket),
            row.ConfirmedReservationsCount.ToString(CultureInfo.InvariantCulture)
        });

        return ToCsvBytes(GenerateCsv(RestaurantPerformanceHeaders, csvRows));
    }

    public byte[] ExportGlobalSales(List<GlobalSalesRow> rows)
    {
        var csvRows = rows.Select(row => new[]
        {
            row.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            row.RestaurantName,
            row.OrdersCount.ToString(CultureInfo.InvariantCulture),
            FormatDecimal(row.TotalRevenue)
        });

        return ToCsvBytes(GenerateCsv(GlobalSalesHeaders, csvRows));
    }

    // Escapa un campo: si contiene caracteres especiales lo envuelve en comillas dobles
    // y duplica las comillas internas. Un campo sin caracteres especiales no se toca.
    private static string EscapeField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }

    // Formatea decimales con cultura invariante (punto decimal) para conciliaciones fuera del sistema,
    // devolviendo vacío cuando el valor es nulo (ejemplo: calificación pendiente)
    private static string FormatDecimal(decimal? value)
    {
        return value?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    // Convierte el texto CSV a bytes UTF-8 anteponiendo el BOM
    private static byte[] ToCsvBytes(string csv)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(csv);
        var result = new byte[preamble.Length + body.Length];
        preamble.CopyTo(result, 0);
        body.CopyTo(result, preamble.Length);
        return result;
    }
}
