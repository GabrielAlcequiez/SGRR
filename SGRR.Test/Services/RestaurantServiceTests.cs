using Microsoft.EntityFrameworkCore;
using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using SGRR.Web.Models.Restaurant;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class RestaurantServiceTests : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _context;
    private readonly RestaurantService _service;
    private readonly User _owner;

    public RestaurantServiceTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new AppDbContext(_options);
        _owner = new User("Owner", "", "owner@test.com", "123456", Roles.Propietary);
        _context.Users.Add(_owner);
        _context.SaveChanges();
        _service = new RestaurantService(new RestaurantRepository(_context), new UserRepository(_context));
    }

    [Fact]
    public async Task CreateAsync_AsociaRestauranteYLoCargaEnOtraSolicitud()
    {
        var restaurant = await _service.CreateAsync(ValidProfile(), _owner);
        using var nextRequest = new AppDbContext(_options);
        var owner = new UserRepository(nextRequest).GetById(_owner.Id);
        Assert.Equal(restaurant.Id, owner!.Restaurant!.Id);
        Assert.Single(await _service.GetAllAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(ValidProfile(), owner));
    }

    [Theory]
    [InlineData("", "Carnes", "Santo Domingo")]
    [InlineData("La Parrilla", "   ", "Santo Domingo")]
    [InlineData("La Parrilla", "Carnes", "")]
    public async Task CreateAsync_SinCampoObligatorio_Rechaza(string name, string specialty, string location)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(
            new CreateRestaurantDto { Name = name, Specialty = specialty, Location = location }, _owner));
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task UpdateAsync_PerfilPropio_ApareceActualizadoEnBusqueda()
    {
        var restaurant = await _service.CreateAsync(ValidProfile(), _owner);
        await _service.UpdateAsync(new UpdateRestaurantDto
        {
            Id = restaurant.Id, Name = "  Nuevo nombre  ", Specialty = " Italiana ", Location = " Santiago "
        }, _owner);
        using var nextRequest = new AppDbContext(_options);
        var service = new RestaurantService(new RestaurantRepository(nextRequest), new UserRepository(nextRequest));
        var found = Assert.Single(await service.SearchAsync("  ITALIANA  "));
        Assert.Equal("Nuevo nombre", found.Name);
        Assert.Equal("Santiago", found.Location);
        Assert.Empty(await service.SearchAsync("No coincide"));
    }

    [Fact]
    public async Task UpdateAsync_ConCamposInvalidos_NoModificaPerfil()
    {
        var restaurant = await _service.CreateAsync(ValidProfile(), _owner);
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(new UpdateRestaurantDto
        {
            Id = restaurant.Id, Name = "Otro nombre", Specialty = "", Location = "Santiago"
        }, _owner));
        Assert.Equal("La Parrilla", restaurant.Name);
    }

    [Fact]
    public async Task EditarOEliminarRestauranteAjeno_Rechaza()
    {
        var restaurant = await _service.CreateAsync(ValidProfile(), _owner);
        var otherOwner = new User("Other", "", "other@test.com", "123456", Roles.Propietary);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateAsync(new UpdateRestaurantDto
        {
            Id = restaurant.Id, Name = "Otro", Specialty = "Carnes", Location = "Santiago"
        }, otherOwner));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(restaurant.Id, otherOwner));
        Assert.Single(await _service.GetAllAsync());
    }

    [Fact]
    public async Task ClienteOPropietarioInactivo_NoPuedenCrearPerfil()
    {
        var client = new User("Client", "", "client@test.com", "123456", Roles.Client);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(ValidProfile(), client));
        _owner.SetActive(false);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateAsync(ValidProfile(), _owner));
    }

    [Fact]
    public async Task DeleteAsync_EliminaPlatosYPermiteCrearNuevoPerfil()
    {
        var restaurant = await _service.CreateAsync(ValidProfile(), _owner);
        _context.MenuItems.Add(new MenuItem("Pizza", "Pizza de queso", 100, "Platos Fuertes", restaurant.Id));
        await _context.SaveChangesAsync();
        await _service.DeleteAsync(restaurant.Id, _owner);
        using var nextRequest = new AppDbContext(_options);
        var owner = new UserRepository(nextRequest).GetById(_owner.Id);
        Assert.Null(owner!.Restaurant);
        Assert.Empty(nextRequest.MenuItems);
        Assert.Empty(await _service.GetAllAsync());
        await _service.CreateAsync(ValidProfile(), _owner);
    }

    private static CreateRestaurantDto ValidProfile() => new()
    {
        Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo"
    };

    public void Dispose() => _context.Dispose();
}
