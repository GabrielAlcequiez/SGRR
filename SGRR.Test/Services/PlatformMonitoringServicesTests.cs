using Moq;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Services;

namespace SGRR.Test.Services
{
    public class PlatformMonitoringServicesTests
    {
        [Theory]
        [InlineData(Roles.Client)]
        [InlineData(Roles.Propietary)]
        public async Task GetSystemHealthAsync_NonAdministrator_ThrowsUnauthorizedAccessException(Roles role)
        {
            // Arrange
            var user = new User(
                "Juan",
                "Pérez",
                "juan@test.com",
                "Test123!",
                role
            );

            var service = new PlatformMonitoringService(
                Mock.Of<IUserRepository>(),
                Mock.Of<IRestaurantRepository>(),
                Mock.Of<IOrderRepository>(),
                Mock.Of<ISystemActivityLogRepository>()
            );

            // Act
            Func<Task> act = () => service.GetSystemHealthAsync(user);

            // Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        }
    }
}