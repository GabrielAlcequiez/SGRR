using System.Text;
using SGRR.Web.Models.Reports;
using SGRR.Web.Services;

namespace SGRR.Test.Services
{
    public class ReportExportServiceTests
    {
        [Fact]
        public void GenerateCsv_FieldContainsComma_EnclosesFieldInDoubleQuotes()
        {
            // Arrange
            var service = new ReportExportService();

            string[] headers = ["Restaurante", "Ubicacion"];
            string[][] rows =
            [
                ["La Casa, Restaurante", "Santo Domingo"]
            ];

            // Act
            var result = service.GenerateCsv(headers, rows);

            // Assert
            var expected = "\"La Casa, Restaurante\",Santo Domingo";

            Assert.Contains(expected, result);
        }

        [Fact]
        public void ExportGlobalSales_EmptyList_ReturnsCsvWithHeaders()
        {
            // Arrange
            var service = new ReportExportService();
            var rows = new List<GlobalSalesRow>();

            // Act
            var result = service.ExportGlobalSales(rows);

            var csv = Encoding.UTF8.GetString(result)
                .TrimStart('\uFEFF')
                .TrimEnd('\r', '\n');

            // Assert
            Assert.Equal(
                "Fecha,Restaurante,Cantidad de Pedidos,Ingresos Totales",
                csv
            );

            Assert.NotEmpty(result);
        }
    }
}