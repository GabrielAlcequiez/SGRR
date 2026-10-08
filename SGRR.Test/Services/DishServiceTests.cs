using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using SGRR.Web.Models.Dish;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class DishServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DishService _service;
    private readonly User _owner;
    private readonly User _otherOwner;

    public DishServiceTests()
    {
        _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _owner = new User("Owner", "", "owner@test.com", "123456", Roles.Propietary);
        _owner.SetRestaurant(new Restaurant { Name = "Restaurante A" });
        _otherOwner = new User("Other", "", "other@test.com", "123456", Roles.Propietary);
        _otherOwner.SetRestaurant(new Restaurant { Name = "Restaurante B" });
        _context.Users.AddRange(_owner, _otherOwner);
        _context.SaveChanges();
        _service = new DishService(new DishRepository(_context));
    }

    [Fact]
    public async Task CreateAsync_ConDatosValidos_UsaRestauranteDelPropietario()
    {
        var dish = await Create("  Pizza Margarita  ");
        Assert.NotEqual(Guid.Empty, dish.Id);
        Assert.Equal("Pizza Margarita", dish.Name);
        Assert.Equal(_owner.Restaurant!.Id, dish.RestaurantId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task CreateAsync_ConPrecioInvalido_LanzaArgumentException(decimal price)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(new CreateDishDto { Name = "Pizza", Price = price }, _owner));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_SinNombre_LanzaArgumentException(string name)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Create(name));
    }

    [Fact]
    public async Task CreateAsync_ConNombreDuplicadoIgnorandoEspaciosYMayusculas_Rechaza()
    {
        await Create("Pizza Margarita");
        await Assert.ThrowsAsync<ArgumentException>(() => Create("  PIZZA MARGARITA  "));
    }

    [Fact]
    public async Task CreateAsync_MismoNombreEnOtroRestaurante_Permite()
    {
        await Create("Pizza");
        await _service.CreateAsync(new CreateDishDto { Name = "Pizza", Price = 100 }, _otherOwner);
        Assert.Single(await _service.GetByRestaurantIdAsync(_owner));
        Assert.Single(await _service.GetByRestaurantIdAsync(_otherOwner));
    }

    [Fact]
    public async Task UpdateAsync_ConNombreDeOtroPlato_RechazaSinModificar()
    {
        await Create("Pizza");
        var dish = await Create("Pasta");
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(
            new UpdateDishDto { Id = dish.Id, Name = "  PIZZA  ", Price = 200 }, _owner));
        Assert.Equal("Pasta", dish.Name);
    }

    [Fact]
    public async Task UpdateAsync_ConElMismoNombreYDatosValidos_Guarda()
    {
        var dish = await Create("Pizza");
        await _service.UpdateAsync(new UpdateDishDto { Id = dish.Id, Name = " Pizza ", Price = 400 }, _owner);
        var saved = await _service.GetByIdAsync(dish.Id, _owner);
        Assert.Equal("Pizza", saved!.Name);
        Assert.Equal(400, saved.Price);
    }

    [Fact]
    public async Task OperacionesSobrePlatoAjeno_RechazanSinModificarNiEliminar()
    {
        var dish = await Create("Pizza");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetByIdAsync(dish.Id, _otherOwner));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateAsync(
            new UpdateDishDto { Id = dish.Id, Name = "Otro", Price = 10 }, _otherOwner));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(dish.Id, _otherOwner));
        Assert.Equal("Pizza", (await _service.GetByIdAsync(dish.Id, _owner))!.Name);
    }

    [Fact]
    public async Task DeleteAsync_PlatoPropio_DesapareceDelMenu()
    {
        var dish = await Create("Pizza");
        await _service.DeleteAsync(dish.Id, _owner);
        Assert.Empty(await _service.GetByRestaurantIdAsync(_owner));
    }

    [Fact]
    public async Task ClientePropietarioInactivoOSinRestaurante_NoPuedenCrear()
    {
        var client = new User("Client", "", "client@test.com", "123456", Roles.Client);
        var ownerWithoutRestaurant = new User("Owner", "", "new@test.com", "123456", Roles.Propietary);
        var dto = new CreateDishDto { Name = "Pizza", Price = 100 };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(dto, client));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(dto, ownerWithoutRestaurant));
        _owner.SetActive(false);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(dto, _owner));
    }

    private Task<Dish> Create(string name) =>
        _service.CreateAsync(new CreateDishDto { Name = name, Price = 350 }, _owner);

    public void Dispose() => _context.Dispose();
}
