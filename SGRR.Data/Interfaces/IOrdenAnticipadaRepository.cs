using SGRR.Data.Entities;

namespace SGRR.Data.Interfaces;

public interface IOrdenAnticipadaRepository
{
    Task<OrdenAnticipada?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrdenAnticipada>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<OrdenAnticipada>> GetByRestaurantIdAsync(Guid restaurantId);
    Task AddAsync(OrdenAnticipada orden);
    Task<bool> UpdateAsync(OrdenAnticipada orden);
    Task<int> CountByYearAsync(int year);
}
