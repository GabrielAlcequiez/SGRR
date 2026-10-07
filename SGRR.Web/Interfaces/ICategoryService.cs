using SGRR.Data.Entities;

namespace SGRR.Web.Interfaces;

public interface ICategoryService
{
    IEnumerable<Category> GetAllCategories(User requestor);
    void CreateCategory(string name, User requestor);
    Task DeleteCategoryAsync(Guid categoryId, User requestor);
}