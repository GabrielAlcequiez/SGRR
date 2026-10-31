using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Models;
using SGRR.Data.Repositories;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class RestaurantSearchServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RestaurantSearchService _service;

    public RestaurantSearchServiceTests()
    {
        _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        _context.Restaurants.AddRange(
            new Restaurant { Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo", Rating = 4.5m },
            new Restaurant { Name = "Pasta Bella", Specialty = "Italiana", Location = "Santiago", Rating = 3.0m },
            new Restaurant { Name = "Sushi Zen", Specialty = "Japonesa", Location = "Santo Domingo", Rating = 2.0m }
        );
        _context.SaveChanges();

        var repository = new RestaurantRepository(_context);
        _service = new RestaurantSearchService(repository);
    }

    [Fact]
    public async Task SearchAsync_ConFiltroCombinado_RetornaSoloCoincidencias()
    {
        var filtro = new RestaurantSearchFilterDto
        {
            Location = "Santo Domingo",
            MinimumRating = 4
        };

        var resultado = await _service.SearchAsync(filtro);

        Assert.Single(resultado);
        Assert.Equal("La Parrilla", resultado.First().Name);
    }

    [Fact]
    public async Task SearchAsync_SinCoincidencias_RetornaListaVacia()
    {
        var filtro = new RestaurantSearchFilterDto
        {
            Specialty = "Mexicana"
        };

        var resultado = await _service.SearchAsync(filtro);

        Assert.Empty(resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task SearchAsync_ConEvaluacionFueraDeRango_LanzaArgumentOutOfRangeException(decimal rating)
    {
        var filtro = new RestaurantSearchFilterDto
        {
            MinimumRating = rating
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _service.SearchAsync(filtro));
    }

    public void Dispose() => _context.Dispose();
}
