using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Web.Services;

namespace SGRR.Test.Services;

public class AdminReportServiceTests : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _context;
    private readonly AdminReportService _service;

    public AdminReportServiceTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        _context = new AppDbContext(_options);
        _service = new AdminReportService(_context);
    }

    [Fact]
    public async Task GetRestaurantPerformanceAsync_OrdersByRevenueDescending()
    {
        // Arrange
        var laParrilla = new Restaurant { Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo" };
        var laTrattoria = new Restaurant { Name = "La Trattoria", Specialty = "Italiana", Location = "Santo Domingo" };
        _context.Restaurants.AddRange(laParrilla, laTrattoria);
        _context.Orders.AddRange(
            new Order { RestaurantId = laParrilla.Id, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.Today },
            new Order { RestaurantId = laParrilla.Id, TotalAmount = 50, Status = "Completed", CreatedAt = DateTime.Today },
            new Order { RestaurantId = laTrattoria.Id, TotalAmount = 80, Status = "Completed", CreatedAt = DateTime.Today });
        await _context.SaveChangesAsync();

        // Act
        var rows = await _service.GetRestaurantPerformanceAsync(DateTime.Today, DateTime.Today);

        // Assert
        Assert.Equal(2, rows.Count);
        Assert.Equal("La Parrilla", rows[0].RestaurantName);
        Assert.Equal(2, rows[0].CompletedOrdersCount);
        Assert.Equal(150m, rows[0].TotalRevenue);
        Assert.Equal(75m, rows[0].AverageTicket);
        Assert.Equal("La Trattoria", rows[1].RestaurantName);
        Assert.Equal(1, rows[1].CompletedOrdersCount);
        Assert.Equal(80m, rows[1].TotalRevenue);
        Assert.Equal(80m, rows[1].AverageTicket);
    }

    [Fact]
    public async Task GetRestaurantPerformanceAsync_ExcludesCancelledOrders()
    {
        // Arrange
        var restaurant = new Restaurant { Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo" };
        _context.Restaurants.Add(restaurant);
        _context.Orders.AddRange(
            new Order { RestaurantId = restaurant.Id, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.Today },
            new Order { RestaurantId = restaurant.Id, TotalAmount = 999, Status = "Cancelled", CreatedAt = DateTime.Today });
        await _context.SaveChangesAsync();

        // Act
        var rows = await _service.GetRestaurantPerformanceAsync(DateTime.Today, DateTime.Today);

        // Assert
        var row = Assert.Single(rows);
        Assert.Equal(1, row.CompletedOrdersCount);
        Assert.Equal(100m, row.TotalRevenue);
        Assert.Equal(100m, row.AverageTicket);
    }

    [Fact]
    public async Task GetGlobalSalesAsync_GroupsSameDayOrdersIntoOneRow()
    {
        // Arrange
        var restaurant = new Restaurant { Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo" };
        _context.Restaurants.Add(restaurant);
        _context.Orders.AddRange(
            new Order { RestaurantId = restaurant.Id, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.Today },
            new Order { RestaurantId = restaurant.Id, TotalAmount = 90, Status = "Completed", CreatedAt = DateTime.Today });
        await _context.SaveChangesAsync();

        // Act
        var rows = await _service.GetGlobalSalesAsync(DateTime.Today, DateTime.Today);

        // Assert
        var row = Assert.Single(rows);
        Assert.Equal(restaurant.Name, row.RestaurantName);
        Assert.Equal(DateTime.Today, row.Date);
        Assert.Equal(2, row.OrdersCount);
        Assert.Equal(190m, row.TotalRevenue);
    }

    [Fact]
    public async Task GetGlobalSalesAsync_ExcludesOrdersOutsideDateRange()
    {
        // Arrange
        var restaurant = new Restaurant { Name = "La Parrilla", Specialty = "Carnes", Location = "Santo Domingo" };
        _context.Restaurants.Add(restaurant);
        _context.Orders.AddRange(
            new Order { RestaurantId = restaurant.Id, TotalAmount = 100, Status = "Completed", CreatedAt = DateTime.Today },
            new Order { RestaurantId = restaurant.Id, TotalAmount = 999, Status = "Completed", CreatedAt = DateTime.Today.AddDays(-30) });
        await _context.SaveChangesAsync();

        // Act
        var rows = await _service.GetGlobalSalesAsync(DateTime.Today, DateTime.Today);

        // Assert
        var row = Assert.Single(rows);
        Assert.Equal(1, row.OrdersCount);
        Assert.Equal(100m, row.TotalRevenue);
    }

    public void Dispose() => _context.Dispose();
}
