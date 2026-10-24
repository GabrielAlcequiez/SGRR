using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Models;
using SGRR.Web.Interfaces;
using SGRR.Web.Services;
using Xunit;

namespace SGRR.Test.Services
{
    public class PasarelaPagoSimuladaParaTests : IPaymentGateway
    {
        public bool DebeAprobar { get; set; } = true;
        public string MensajeSimulado { get; set; } = "Aprobado";

        public Task<PaymentGatewayResult> ProcesarAsync(
            PagoTransaccionDto transaccion,
            CancellationToken cancellationToken = default)
        {
            var resultado = new PaymentGatewayResult(DebeAprobar, MensajeSimulado, "REF-SIMULADA-12345");
            return Task.FromResult(resultado);
        }
    }

    public class PagoServiceTests
    {
        private readonly PasarelaPagoSimuladaParaTests _pasarelaSimulada;
        private readonly ILogger<PagoService> _loggerNulo;

        public PagoServiceTests()
        {
            _pasarelaSimulada = new PasarelaPagoSimuladaParaTests();
            using var loggerFactory = new LoggerFactory();
            _loggerNulo = loggerFactory.CreateLogger<PagoService>();
        }

        private AppDbContext ObtenerContextoEnMemoria()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task ProcesarAsync_CuandoMontosNoCoinciden_DeberiaRechazarElPago()
        {
            using var context = ObtenerContextoEnMemoria();
            var clienteId = Guid.NewGuid();
            var ordenId = Guid.NewGuid();

            var orden = new OrdenAnticipada { Id = ordenId, ClientId = clienteId, Total = 1500 };
            await context.OrdenesAnticipadas.AddAsync(orden);
            await context.SaveChangesAsync();

            var service = new PagoService(context, _pasarelaSimulada, _loggerNulo);
            var transaccion = new PagoTransaccionDto { OrdenAnticipadaId = ordenId, Monto = 1000 };

            var resultado = await service.ProcesarAsync(clienteId, transaccion);

            Assert.False(resultado.Aprobado);
            Assert.Equal("El importe del pago no coincide con el total actual de la orden.", resultado.Mensaje);
        }

        [Fact]
        public async Task ProcesarAsync_CuandoTodoEsCorrecto_DeberiaRegistrarElPagoYRetornarExito()
        {
            using var context = ObtenerContextoEnMemoria();
            var clienteId = Guid.NewGuid();
            var ordenId = Guid.NewGuid();

            var orden = new OrdenAnticipada { Id = ordenId, ClientId = clienteId, Total = 3500 };
            await context.OrdenesAnticipadas.AddAsync(orden);
            await context.SaveChangesAsync();

            _pasarelaSimulada.DebeAprobar = true;
            _pasarelaSimulada.MensajeSimulado = "Pago realizado correctamente.";

            var service = new PagoService(context, _pasarelaSimulada, _loggerNulo);
            var transaccion = new PagoTransaccionDto { OrdenAnticipadaId = ordenId, Monto = 3500, MetodoPago = "Tarjeta" };

            var resultado = await service.ProcesarAsync(clienteId, transaccion);

            Assert.True(resultado.Aprobado);
            Assert.Equal("Pago realizado correctamente.", resultado.Mensaje);

            var seGuardoEnDb = await context.Pagos.AnyAsync(p => p.OrdenAnticipadaId == ordenId);
            Assert.True(seGuardoEnDb);
        }
    }
}
