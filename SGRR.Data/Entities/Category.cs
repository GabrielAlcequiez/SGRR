namespace SGRR.Data.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la categoría es obligatorio.", nameof(name));
        
        Id = Guid.NewGuid();
        Name = name.Trim();
    }
}