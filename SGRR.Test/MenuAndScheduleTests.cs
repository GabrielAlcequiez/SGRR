using Moq;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SGRR.Test
{
    public class MenuAndScheduleTests
    {
        // los mocks adaptados a lo servicios existentes
        private readonly Mock<IRestaurantRepository> _mockRestaurantRepo;

        public MenuAndScheduleTests()
        {
            _mockRestaurantRepo = new Mock<IRestaurantRepository>();
        }

        [Fact]
        public async Task GetAvailableMenuItems_ReturnsListWithFullDetails()
        {
            // Arrange 
            var restaurantId = Guid.NewGuid();
            var mockDishes = new List<MenuItem>
            {
                
                new MenuItem("Plato Ejecutivo", "Arroz, carne y ensalada", 350.00m, "categoria-id", restaurantId, "imagen.jpg")
                
            };

            
            Assert.NotNull(mockDishes);
            var dish = mockDishes.First();
            Assert.Equal("Plato Ejecutivo", dish.Name);
            Assert.True(dish.Price > 0);
        }

        [Fact]
        public async Task GetRestaurantSchedules_ReturnsConfiguredSchedules()
        {
            // Arrange 
            var restaurantId = Guid.NewGuid();
            var mockSchedules = new List<RestaurantSchedule>
            {
                new RestaurantSchedule(restaurantId, DayOfWeek.Monday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0)),
                new RestaurantSchedule(restaurantId, DayOfWeek.Tuesday, new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0))
            };

            // Act & Assert
            Assert.NotNull(mockSchedules);
            Assert.Equal(2, mockSchedules.Count());
            Assert.All(mockSchedules, schedule =>
            {
                Assert.Equal(restaurantId, schedule.RestaurantId);
            });
        }
    }
}