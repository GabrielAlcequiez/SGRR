using SGRR.Data.Entities;
using SGRR.Web.Services;

namespace SGRR.Test.Services
{
    public class OwnerMetricServiceTests
    {
        [Fact]
        public void Calculate_CancelledOrders_ExcludesFromRevenue()
        {
            // Arrange
            var orders = new List<Order>
            {
                new() { Status = "Completed", TotalAmount = 500m },
                new() { Status = "Cancelled", TotalAmount = 1000m },
                new() { Status = "Completada", TotalAmount = 300m }
            };

            // Act
            var result = OwnerMetricsService.Calculate(orders);

            // Assert
            Assert.Equal(800m, result.TotalRevenue);
            Assert.Equal(400m, result.AverageTicket);
            Assert.Equal(2, result.CompletedCount);
        }

        [Fact]
        public void Calculate_EmptyOrders_ReturnsZeroMetrics()
        {
            // Arrange
            var orders = new List<Order>();

            // Act
            var result = OwnerMetricsService.Calculate(orders);

            // Assert
            Assert.Equal(0.00m, result.TotalRevenue);
            Assert.Equal(0.00m, result.AverageTicket);
            Assert.Equal(0, result.CompletedCount);
        }
    }
}