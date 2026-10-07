using SGRR.Data.Common.Enums;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;
using SGRR.Web.Interfaces;

namespace SGRR.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public CategoryService(ICategoryRepository categoryRepository, IRestaurantRepository restaurantRepository)
    {
        _categoryRepository = categoryRepository;
        _restaurantRepository = restaurantRepository;
    }

    public IEnumerable<Category> GetAllCategories(User requestor)
    {
        ValidateAdminRole(requestor);
        return _categoryRepository.GetAll();
    }

    public void CreateCategory(string name, User requestor)
    {
        ValidateAdminRole(requestor);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre de la categoría es requerido.");
        }

        if (_categoryRepository.ExistsByName(name))
        {
            throw new InvalidOperationException($"La categoría '{name}' ya existe.");
        }

        var category = new Category(name);
        _categoryRepository.Add(category);
    }

    public async Task DeleteCategoryAsync(Guid categoryId, User requestor)
    {
        ValidateAdminRole(requestor);

        var category = _categoryRepository.GetById(categoryId);
        if (category == null)
        {
            throw new KeyNotFoundException("Categoría no encontrada.");
        }

        // Await para obtener la lista asíncrona de restaurantes
        var allRestaurants = await _restaurantRepository.GetAllAsync();
        bool hasRestaurants = allRestaurants.Any(r => r.CategoryId == categoryId);
        
        if (hasRestaurants)
        {
            throw new InvalidOperationException("No se puede eliminar una categoría que tenga restaurantes asociados.");
        }

        _categoryRepository.Delete(categoryId);
    }

    private void ValidateAdminRole(User user)
    {
        if (user == null || user.Role != Roles.Administrator)
        {
            throw new UnauthorizedAccessException("Acceso denegado. Se requieren permisos de Administrador.");
        }
    }
}