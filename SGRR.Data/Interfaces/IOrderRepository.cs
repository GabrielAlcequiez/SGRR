namespace SGRR.Data.Interfaces;

public interface IOrderRepository
{
    // Simulación del método que buscará órdenes del día
    Task<int> GetDailyTransactionsCountAsync(); 
}