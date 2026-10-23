using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Controllers;
using SGRR.Web.Interfaces;
using SGRR.Web.Models;
using SGRR.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SGRR.Test.Controllers
{
    public class OrdenesAnticipadasControllerTests
    {
        private readonly Mock<IOrdenesAnticipadasRepository> _mockOrdenesRepo;
        private readonly Mock<IDishRepository> _mockDishRepo;
        private readonly OrdenesAnticipadasController _controller;

        public OrdenesAnticipadasControllerTests()
        {
            _mockOrdenesRepo = new Mock<IOrdenesAnticipadasRepository>();
            _mockDishRepo = new Mock<IDishRepository>();

            _controller = new OrdenesAnticipadasController(
                _mockOrdenesRepo.Object,
                _mockDishRepo.Object
            );
        }

        [Fact]
        public async Task Index_CargaProductosEnViewBag_YRetornaVista()
        {
            // Arrange
            var listaPlatillos = new List<Dish>
            {
                new Dish { Id = Guid.NewGuid(), Name = "Plato del Día" },
                new Dish { Id = Guid.NewGuid(), Name = "Menú Ejecutivo" }
            };

            _mockDishRepo.Setup(repo => repo.GetAllAsync())
                         .ReturnsAsync(listaPlatillos);

            // Act
            var result = await _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var productosViewBag = result.ViewData["Productos"] as IEnumerable<SelectListItem>;
            Assert.NotNull(productosViewBag);
        }

        [Fact]
        public async Task Crear_ConModeloValido_GuardaOrden_YEstableceMensajeExito()
        {
            // Arrange 
            var nuevaOrden = new OrdenAnticipada
            {
                Producto = "Plato del Día",
                Cantidad = 2,
                Fecha = DateTime.Today.AddDays(1),
                Hora = new TimeSpan(12, 30, 0)
            };

            // Act
            var result = await _controller.Crear(nuevaOrden) as ViewResult;

            // Assert
            _mockOrdenesRepo.Verify(r => r.Agregar(It.IsAny<OrdenAnticipada>()), Times.Once);
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["SuccessMessage"]);
        }

        [Fact]
        public async Task Crear_ConModeloInvalido_NoGuardaOrden_YRetornaMensajeError()
        {
            // Arrange
            var ordenInvalida = new OrdenAnticipada
            {
                Cantidad = 0
            };

            _controller.ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a 0");

            // Act
            var result = await _controller.Crear(ordenInvalida) as ViewResult;

            // Assert
            _mockOrdenesRepo.Verify(r => r.Agregar(It.IsAny<OrdenAnticipada>()), Times.Never);
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData["ErrorMessage"]);
        }
    }
}