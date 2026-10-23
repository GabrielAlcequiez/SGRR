using Microsoft.AspNetCore.Mvc;
using Moq;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces; 
using SGRR.Web.Controllers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SGRR.Test
{
    public class OrdenesAnticipadasControllerTests
    {
        private readonly Mock<IOrdenAnticipadaService> _mockOrdenService;
        private readonly Mock<IRestaurantRepository> _mockRestaurantRepo;
        private readonly Mock<IMenuRepository> _mockMenuRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly OrdenesAnticipadasController _controller;

        public OrdenesAnticipadasControllerTests()
        {
            _mockOrdenService = new Mock<IOrdenAnticipadaService>();
            _mockRestaurantRepo = new Mock<IRestaurantRepository>();
            _mockMenuRepo = new Mock<IMenuRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            // Ajusta los parámetros 
            _controller = new OrdenesAnticipadasController(
                _mockOrdenService.Object,
                _mockRestaurantRepo.Object,
                _mockMenuRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();

            _mockRestaurantRepo.Setup(r => r.GetByIdAsync(restaurantId))
                               .ReturnsAsync(new Restaurant { Id = restaurantId });

            // Act
            var result = await _controller.Index(restaurantId);

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}