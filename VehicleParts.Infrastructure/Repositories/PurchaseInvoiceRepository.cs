using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class PurchaseInvoiceRepository : IPurchaseInvoiceRepository
{
    private readonly AppDbContext _context;

    public PurchaseInvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PurchaseInvoice>> GetAllAsync()
    {
        return await _context.PurchaseInvoices
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .ToListAsync();
    }

    public async Task<PurchaseInvoice?> GetByIdAsync(int id)
    {
        return await _context.PurchaseInvoices
            .Include(p => p.Vendor)
            .Include(p => p.Items)
                .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PurchaseInvoice> CreateAsync(PurchaseInvoice invoice)
    {
        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<PurchaseInvoice> UpdateAsync(PurchaseInvoice invoice)
    {
        _context.PurchaseInvoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
}