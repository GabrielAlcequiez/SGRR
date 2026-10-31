using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using Xunit;

namespace SGRR.Test.Repositories
{
    public class OrdenAnticipadaRepositoryTests
    {
        // Método para generar una base de datos en memoria limpia y aislada por test
        private AppDbContext ObtenerContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // =================================================================
        // METODO: AddAsync
        // =================================================================

        [Fact]
        public async Task AddAsync_CuandoSeAgregaUnaOrden_DeberiaGuardarseCorrectamente()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var repository = new OrdenAnticipadaRepository(context);
            var nuevaOrden = new OrdenAnticipada
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                RestaurantId = Guid.NewGuid(),
                PurchaseCode = "ORD-2026-001",
                FechaOrden = DateTime.Now
            };

            // Act
            await repository.AddAsync(nuevaOrden);

            // Assert
            var ordenEnDb = await context.OrdenesAnticipadas.FindAsync(nuevaOrden.Id);
            Assert.NotNull(ordenEnDb);
            Assert.Equal(nuevaOrden.PurchaseCode, ordenEnDb.PurchaseCode);
        }

        // =================================================================
        // METODO: GetByIdAsync
        // =================================================================

        [Fact]
        public async Task GetByIdAsync_CuandoLaOrdenExiste_DeberiaRetornarlaExitosamente()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var idBuscado = Guid.NewGuid();
            var orden = new OrdenAnticipada { Id = idBuscado, PurchaseCode = "ORD-2026-B" };
            await context.OrdenesAnticipadas.AddAsync(orden);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var resultado = await repository.GetByIdAsync(idBuscado);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(idBuscado, resultado.Id);
        }

        [Fact]
        public async Task GetByIdAsync_CuandoLaOrdenNoExiste_DeberiaRetornarNull()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var resultado = await repository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(resultado);
        }

        // =================================================================
        // METODO: GetByClientIdAsync
        // =================================================================

        [Fact]
        public async Task GetByClientIdAsync_DeberiaFiltrarYRetornarSoloLasOrdenesDelCliente()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var clienteTarget = Guid.NewGuid();
            var orden1 = new OrdenAnticipada { Id = Guid.NewGuid(), ClientId = clienteTarget, FechaOrden = DateTime.Now };
            var ordenDeOtro = new OrdenAnticipada { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), FechaOrden = DateTime.Now };

            await context.OrdenesAnticipadas.AddRangeAsync(orden1, ordenDeOtro);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var resultado = await repository.GetByClientIdAsync(clienteTarget);

            // Assert
            Assert.Single(resultado);
            Assert.All(resultado, o => Assert.Equal(clienteTarget, o.ClientId));
        }

        [Fact]
        public async Task GetByClientIdAsync_DeberiaRetornarLasOrdenesOrdenadasDeLaMasRecienteALaMasAntigua()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var clienteId = Guid.NewGuid();
            var ordenAntigua = new OrdenAnticipada { Id = Guid.NewGuid(), ClientId = clienteId, FechaOrden = DateTime.Now.AddDays(-5) };
            var ordenNueva = new OrdenAnticipada { Id = Guid.NewGuid(), ClientId = clienteId, FechaOrden = DateTime.Now };

            await context.OrdenesAnticipadas.AddRangeAsync(ordenAntigua, ordenNueva);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var resultado = (await repository.GetByClientIdAsync(clienteId)).ToList();

            // Assert
            Assert.Equal(ordenNueva.Id, resultado[0].Id); // La primera debe ser la más reciente
        }

        // =================================================================
        // METODO: GetByRestaurantIdAsync
        // =================================================================

        [Fact]
        public async Task GetByRestaurantIdAsync_DeberiaRetornarSoloLasOrdenesDelRestaurante()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var restauranteId = Guid.NewGuid();
            var ordenRestaurante = new OrdenAnticipada { Id = Guid.NewGuid(), RestaurantId = restauranteId, FechaOrden = DateTime.Now };
            var ordenOtro = new OrdenAnticipada { Id = Guid.NewGuid(), RestaurantId = Guid.NewGuid(), FechaOrden = DateTime.Now };

            await context.OrdenesAnticipadas.AddRangeAsync(ordenRestaurante, ordenOtro);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var resultado = await repository.GetByRestaurantIdAsync(restauranteId);

            // Assert
            var elemento = Assert.Single(resultado);
            Assert.Equal(restauranteId, elemento.RestaurantId);
        }

        // =================================================================
        // METODO: UpdateAsync
        // =================================================================

        [Fact]
        public async Task UpdateAsync_CuandoSeModificaUnCampo_DeberiaGuardarElCambioYRetornarTrue()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var orden = new OrdenAnticipada { Id = Guid.NewGuid(), PurchaseCode = "ORIGINAL" };
            await context.OrdenesAnticipadas.AddAsync(orden);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            orden.PurchaseCode = "MODIFICADO";
            var resultado = await repository.UpdateAsync(orden);

            // Assert
            Assert.True(resultado);
            var ordenEnDb = await context.OrdenesAnticipadas.FindAsync(orden.Id);
            Assert.Equal("MODIFICADO", ordenEnDb.PurchaseCode);
        }

        // =================================================================
        // METODO: CountByYearAsync
        // =================================================================

        [Fact]
        public async Task CountByYearAsync_DeberiaContarSoloLasOrdenesQueEmpiecenConElPrefijoDelAnio()
        {
            // Arrange
            using var context = ObtenerContextoEnMemoria();
            var ok1 = new OrdenAnticipada { Id = Guid.NewGuid(), PurchaseCode = "ORD-2026-01" };
            var ok2 = new OrdenAnticipada { Id = Guid.NewGuid(), PurchaseCode = "ORD-2026-02" };
            var añoMal = new OrdenAnticipada { Id = Guid.NewGuid(), PurchaseCode = "ORD-2025-01" };

            await context.OrdenesAnticipadas.AddRangeAsync(ok1, ok2, añoMal);
            await context.SaveChangesAsync();

            var repository = new OrdenAnticipadaRepository(context);

            // Act
            var total = await repository.CountByYearAsync(2026);

            // Assert
            Assert.Equal(2, total);
        }
    }
}
