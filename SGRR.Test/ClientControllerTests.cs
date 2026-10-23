using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Data.Models;
using SGRR.Web.Controllers;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SGRR.Test
{
    public class ClientControllerTests
    {
        private readonly Mock<IRestaurantSearchService> _mockRestaurantService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _mockRestaurantService = new Mock<IRestaurantSearchService>();
            _mockUserRepository = new Mock<IUserRepository>();

            _controller = new ClientController(
                _mockRestaurantService.Object,
                _mockUserRepository.Object
            );
        }

        private void ConfigurarSesion(string? userIdValue)
        {
            var mockSession = new Mock<ISession>();
            byte[]? valueBytes = userIdValue != null ? Encoding.UTF8.GetBytes(userIdValue) : null;

            mockSession.Setup(s => s.TryGetValue("UserId", out valueBytes))
                       .Returns(userIdValue != null);

            var httpContext = new DefaultHttpContext
            {
                Session = mockSession.Object
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_SinSesion_RedirigeALogin()
        {
            // Arrange
            ConfigurarSesion(null);

            // Act
            var result = await _controller.Index(new RestaurantSearchViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Auth", result.ControllerName);
        }

        [Fact]
        public async Task Index_ClienteInexistente_RedirigeALogin()
        {
            // Arrange
            var userId = Guid.NewGuid();
            ConfigurarSesion(userId.ToString());

            _mockUserRepository.Setup(r => r.GetById(userId))
                               .Returns((User?)null);

            // Act
            var result = await _controller.Index(new RestaurantSearchViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ActionName);
            Assert.Equal("Auth", result.ControllerName);
        }

        [Fact]
        public async Task Index_UsuarioNoEsClienteONoEstaActivo_Retorna403Forbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            ConfigurarSesion(userId.ToString());

            //constructor real de User
            var usuarioInvalido = new User("Admin", "User", "admin@sgrr.com", "Password123!", Roles.Administrator)
            {
                Id = userId
            };


            _mockUserRepository.Setup(r => r.GetById(userId))
                               .Returns(usuarioInvalido);

            // Act
            var result = await _controller.Index(new RestaurantSearchViewModel()) as StatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task Index_ClienteValido_RetornaVistaConResultados()
        {
            // Arrange
            var userId = Guid.NewGuid();
            ConfigurarSesion(userId.ToString());


            var clienteValido = new User("Juan", "Pérez", "juan@sgrr.com", "Password123!", Roles.Client)
            {
                Id = userId
            };

            _mockUserRepository.Setup(r => r.GetById(userId))
                               .Returns(clienteValido);

            _mockRestaurantService.Setup(s => s.SearchAsync(It.IsAny<RestaurantSearchFilterDto>()))
                                  .ReturnsAsync(new List<Restaurant>());

            var model = new RestaurantSearchViewModel { Specialty = "Pizza" };

            // Act
            var result = await _controller.Index(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            _mockRestaurantService.Verify(s => s.SearchAsync(It.IsAny<RestaurantSearchFilterDto>()), Times.Once);
        }
    }
}