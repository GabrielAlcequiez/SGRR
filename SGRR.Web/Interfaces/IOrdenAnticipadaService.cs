using SGRR.Web.Models.Ordenes;

namespace SGRR.Web.Interfaces;

public interface IOrdenAnticipadaService
{
    Task<string> CrearAsync(CreateOrdenAnticipadaDto dto, Guid clientId);
    Task<IEnumerable<OrdenAnticipadaViewModel>> GetByClientIdAsync(Guid clientId);
    Task<IEnumerable<OrdenAnticipadaViewModel>> GetByRestaurantIdAsync(Guid restaurantId);
    Task<bool> AceptarAsync(Guid ordenId, Guid restaurantId);
    Task<bool> RechazarAsync(Guid ordenId, Guid restaurantId);
    Task<bool> MarcarEntregadaAsync(Guid ordenId, Guid restaurantId);
}
