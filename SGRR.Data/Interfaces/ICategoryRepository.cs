using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface ICategoryRepository
{
    void Add(Category category);
    void Delete(Guid id);
    Category? GetById(Guid id);
    IEnumerable<Category> GetAll();
    bool ExistsByName(string name);
}