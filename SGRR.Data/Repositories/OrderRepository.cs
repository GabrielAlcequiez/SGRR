using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> GetDailyTransactionsCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return _context.Orders.CountAsync(order =>
            order.CreatedAt >= today && order.CreatedAt < tomorrow);
    }
}