namespace SGRR.Data.Entities;

public class RestaurantSchedule
{
    public Guid Id { get; private set; }
    public Guid RestaurantId { get; private set; }
    public Restaurant? Restaurant { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan OpeningTime { get; private set; }
    public TimeSpan ClosingTime { get; private set; }
    public bool IsActive { get; private set; } = true;

    public RestaurantSchedule(
        Guid restaurantId,
        DayOfWeek dayOfWeek,
        TimeSpan openingTime,
        TimeSpan closingTime)
    {
        if (restaurantId == Guid.Empty)
            throw new ArgumentException("El ID del restaurante no es válido.", nameof(restaurantId));

        if (openingTime >= closingTime)
            throw new ArgumentException("La hora de apertura debe ser menor a la hora de cierre.");

        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        DayOfWeek = dayOfWeek;
        OpeningTime = openingTime;
        ClosingTime = closingTime;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
