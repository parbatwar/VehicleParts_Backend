using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .ToListAsync();
    }

    public async Task<Customer?> GetByUserIdAsync(long userId)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    
    public async Task<Customer?> GetByIdWithHistoryAsync(int id)
    {
        return await _context.Customers
            .Include(c => c.User)
            .Include(c => c.Vehicles)
            .Include(c => c.SalesInvoices)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
