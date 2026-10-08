using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    public Task<int> GetDailyTransactionsCountAsync()
    {
        // Retornamos 0 temporalmente hasta que se desarrolle el RF05 (Órdenes)
        return Task.FromResult(0);
    }
}