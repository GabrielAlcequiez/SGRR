using SGRR.Data.Common.Enums;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Repositories;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class CategoryServiceTests
{
    private readonly CategoryService _service;
    private readonly InMemoryCategoryRepository _categoryRepository;
    private readonly User _admin;
    private readonly User _client;

    public CategoryServiceTests()
    {
        var context = new AppDbContext();
        context.Database.EnsureDeleted();

        _categoryRepository = new InMemoryCategoryRepository();
        var restaurantRepository = new RestaurantRepository(context);

        _service = new CategoryService(_categoryRepository, restaurantRepository);

        _admin = new User("Admin", "Uno", "admin@foodbook.com", "clave123", Roles.Administrator);
        _client = new User("Cliente", "Uno", "cliente@foodbook.com", "clave123", Roles.Client);
    }

    [Fact]
    public void CreateCategory_ConNombreValidoYAdmin_CreaLaCategoria()
    {
        _service.CreateCategory("Comida Italiana", _admin);

        var categorias = _service.GetAllCategories(_admin);
        Assert.Single(categorias);
        Assert.Equal("Comida Italiana", categorias.First().Name);
    }

    [Fact]
    public void CreateCategory_ConNombreDuplicado_LanzaInvalidOperationException()
    {
        _service.CreateCategory("Comida Italiana", _admin);

        Assert.Throws<InvalidOperationException>(() =>
            _service.CreateCategory("Comida Italiana", _admin));
    }

    [Fact]
    public void CreateCategory_ConUsuarioNoAdmin_LanzaUnauthorizedAccessException()
    {
        Assert.Throws<UnauthorizedAccessException>(() =>
            _service.CreateCategory("Comida Italiana", _client));
    }

    [Fact]
    public async Task DeleteCategoryAsync_SinRestaurantesAsociados_EliminaLaCategoria()
    {
        _service.CreateCategory("Comida Italiana", _admin);
        var categoria = _service.GetAllCategories(_admin).First();

        await _service.DeleteCategoryAsync(categoria.Id, _admin);

        Assert.Empty(_service.GetAllCategories(_admin));
    }
}
