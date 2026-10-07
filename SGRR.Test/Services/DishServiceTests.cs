using SGRR.Data.Context;
using SGRR.Data.Repositories;
using SGRR.Web.Models.Dish;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class DishServiceTests
{
    private readonly DishService _service;
    private readonly Guid _restaurantId = Guid.NewGuid();

    public DishServiceTests()
    {
        var context = new AppDbContext();
        context.Database.EnsureDeleted();

        var repository = new DishRepository(context);
        _service = new DishService(repository);
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_CreaElPlato()
    {
        var dto = new CreateDishDto
        {
            RestaurantId = _restaurantId,
            Name = "Pizza Margarita",
            Price = 350
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Pizza Margarita", result.Name);
    }

    [Fact]
    public async Task CreateAsync_ConPrecioNegativo_LanzaArgumentException()
    {
        var dto = new CreateDishDto
        {
            RestaurantId = _restaurantId,
            Name = "Pizza Margarita",
            Price = -10
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ConNombreDuplicado_LanzaArgumentException()
    {
        var dto = new CreateDishDto
        {
            RestaurantId = _restaurantId,
            Name = "Pizza Margarita",
            Price = 350
        };

        await _service.CreateAsync(dto);

        var dtoDuplicado = new CreateDishDto
        {
            RestaurantId = _restaurantId,
            Name = "Pizza Margarita",
            Price = 400
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dtoDuplicado));
    }
}
