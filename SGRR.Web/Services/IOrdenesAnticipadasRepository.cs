using SGRR.Web.Models;

namespace SGRR.Web.Services
{
    public interface IOrdenesAnticipadasRepository
    {
        IEnumerable<OrdenAnticipada> ObtenerTodas();
        OrdenAnticipada? ObtenerPorId(int id);
        void Agregar(OrdenAnticipada orden);
    }
}