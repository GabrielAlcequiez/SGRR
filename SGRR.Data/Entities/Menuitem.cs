namespace SGRR.Data.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public string ImageUrl { get; private set; }
    public string Category { get; private set; } // Ejemplo: "Entradas", "Bebidas"
    public bool IsAvailable { get; private set; } = true;

    // Relación con el restaurante usando Guid
    public Guid RestaurantId { get; private set; }
    public Restaurant? Restaurant { get; private set; }

    // Constructor estricto con validaciones (Igual al de tu clase User)
    public MenuItem(
        string name,
        string description,
        decimal price,
        string category,
        Guid restaurantId,
        string imageUrl = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del platillo es requerido.", nameof(name));

        if (price <= 0)
            throw new ArgumentException("El precio debe ser mayor a cero.", nameof(price));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("La categoría es requerida.", nameof(category));

        if (restaurantId == Guid.Empty)
            throw new ArgumentException("El ID del restaurante asociado no es válido.", nameof(restaurantId));

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        Category = category.Trim();
        RestaurantId = restaurantId;
        ImageUrl = imageUrl.Trim();
    }

    // Métodos de comportamiento para modificar el estado de forma segura
    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentException("El nuevo precio debe ser mayor a cero.", nameof(newPrice));
        Price = newPrice;
    }
}
