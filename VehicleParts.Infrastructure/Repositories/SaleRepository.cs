using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _context;

    public SaleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesInvoice>> GetAllAsync()
    {
        return await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Include(s => s.Staff)
                .ThenInclude(st => st.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Part)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<SalesInvoice?> GetByIdAsync(int id)
    {
        return await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Include(s => s.Staff)
                .ThenInclude(st => st.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<SalesInvoice>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Include(s => s.Staff)
                .ThenInclude(st => st.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    public async Task<SalesInvoice> CreateAsync(SalesInvoice invoice)
    {
        _context.SalesInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
}