using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Sale;
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

    public async Task<SalesInvoice> UpdateAsync(SalesInvoice invoice)
    {
        _context.SalesInvoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    // Feature 9: Report Queries (Appended to match SaleService)

    public async Task<List<RegularCustomerReportDto>> GetRegularCustomersAsync(int minPurchases)
    {
        return await _context.SalesInvoices
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .GroupBy(si => si.Customer)
            .Where(g => g.Count() >= minPurchases)
            .Select(g => new RegularCustomerReportDto
            {
                CustomerId = g.Key.Id,
                Name = $"{g.Key.User.FirstName} {g.Key.User.LastName}",
                Email = g.Key.User.Email ?? string.Empty,
                Phone = g.Key.User.PhoneNumber ?? string.Empty,
                TotalPurchases = g.Count(),
                LastPurchaseDate = g.Max(si => si.Date) 
            })
            .OrderByDescending(x => x.TotalPurchases)
            .ToListAsync();
    }

    public async Task<List<HighSpenderReportDto>> GetHighSpendersAsync(int topN)
    {
        return await _context.SalesInvoices
            .Include(si => si.Customer).ThenInclude(c => c.User)
            .GroupBy(si => si.Customer)
            .Select(g => new HighSpenderReportDto
            {
                CustomerId = g.Key.Id,
                Name = $"{g.Key.User.FirstName} {g.Key.User.LastName}",
                Email = g.Key.User.Email ?? string.Empty,
                TotalSpent = g.Sum(si => si.TotalAmount),
                TotalOrders = g.Count()
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(topN)
            .ToListAsync();
    }

    public async Task<List<PendingCreditReportDto>> GetPendingCreditsAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .Where(c => c.CreditBalance > 0)
            .Select(c => new PendingCreditReportDto
            {
                CustomerId = c.Id,
                Name = $"{c.User.FirstName} {c.User.LastName}",
                Email = c.User.Email ?? string.Empty,
                Phone = c.User.PhoneNumber ?? string.Empty,
                CreditBalance = c.CreditBalance
            })
            .OrderByDescending(c => c.CreditBalance)
            .ToListAsync();
    }
}