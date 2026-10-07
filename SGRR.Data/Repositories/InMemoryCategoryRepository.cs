using System.Collections.Concurrent;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly ConcurrentDictionary<Guid, Category> _categories = new();

    public void Add(Category category)
    {
        _categories.TryAdd(category.Id, category);
    }

    public void Delete(Guid id)
    {
        _categories.TryRemove(id, out _);
    }

    public Category? GetById(Guid id)
    {
        _categories.TryGetValue(id, out var category);
        return category;
    }

    public IEnumerable<Category> GetAll()
    {
        return _categories.Values;
    }

    public bool ExistsByName(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return _categories.Values.Any(c => c.Name.ToLowerInvariant() == normalizedName);
    }
}