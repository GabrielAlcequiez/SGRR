using System.Collections.Concurrent;
using SGRR.Web.Models;

namespace SGRR.Web.Services
{
    public class OrdenesAnticipadasRepository : IOrdenesAnticipadasRepository
    {
        private readonly ConcurrentBag<OrdenAnticipada> _ordenes = new();
        private int _nextId = 1;

        public IEnumerable<OrdenAnticipada> ObtenerTodas()
        {
            return _ordenes.OrderByDescending(o => o.FechaCreacion);
        }

        public OrdenAnticipada? ObtenerPorId(int id)
        {
            return _ordenes.FirstOrDefault(o => o.Id == id);
        }

        public void Agregar(OrdenAnticipada orden)
        {
            orden.Id = Interlocked.Increment(ref _nextId);
            _ordenes.Add(orden);
        }
    }
}