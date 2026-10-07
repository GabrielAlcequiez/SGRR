using SGRR.Data.Context;
using SGRR.Data.Repositories;
using SGRR.Web.Models.Restaurant;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class RestaurantServiceTests
{
    private readonly RestaurantService _service;

    public RestaurantServiceTests()
    {
        var context = new AppDbContext();
        context.Database.EnsureDeleted();

        var repository = new RestaurantRepository(context);
        _service = new RestaurantService(repository);
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_CreaElRestaurante()
    {
        var dto = new CreateRestaurantDto
        {
            Name = "La Parrilla",
            Specialty = "Carnes",
            Location = "Santo Domingo"
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("La Parrilla", result.Name);

        var todos = await _service.GetAllAsync();
        Assert.Single(todos);
    }

    [Fact]
    public async Task CreateAsync_SinNombre_LanzaArgumentException()
    {
        var dto = new CreateRestaurantDto
        {
            Name = "",
            Specialty = "Carnes",
            Location = "Santo Domingo"
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }
}
