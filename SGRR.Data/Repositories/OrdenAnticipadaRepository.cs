using Microsoft.EntityFrameworkCore;
using SGRR.Data.Context;
using SGRR.Data.Entities;
using SGRR.Data.Interfaces;

namespace SGRR.Data.Repositories;

public class OrdenAnticipadaRepository : IOrdenAnticipadaRepository
{
    private readonly AppDbContext _context;

    public OrdenAnticipadaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrdenAnticipada?> GetByIdAsync(Guid id)
    {
        return await _context.OrdenesAnticipadas
            .Include(orden => orden.Productos)
            .FirstOrDefaultAsync(orden => orden.Id == id);
    }

    public async Task<IEnumerable<OrdenAnticipada>> GetByClientIdAsync(Guid clientId)
    {
        return await _context.OrdenesAnticipadas
            .Include(orden => orden.Productos)
            .Where(orden => orden.ClientId == clientId)
            .OrderByDescending(orden => orden.FechaOrden)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrdenAnticipada>> GetByRestaurantIdAsync(Guid restaurantId)
    {
        return await _context.OrdenesAnticipadas
            .Include(orden => orden.Productos)
            .Where(orden => orden.RestaurantId == restaurantId)
            .OrderByDescending(orden => orden.FechaOrden)
            .ToListAsync();
    }

    public async Task AddAsync(OrdenAnticipada orden)
    {
        await _context.OrdenesAnticipadas.AddAsync(orden);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(OrdenAnticipada orden)
    {
        _context.OrdenesAnticipadas.Update(orden);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> CountByYearAsync(int year)
    {
        // Cuenta las órdenes por prefijo del código de compra (ORD-{año}-...)
        var prefix = $"ORD-{year}-";
        return await _context.OrdenesAnticipadas
            .CountAsync(orden => orden.PurchaseCode.StartsWith(prefix));
    }
}
