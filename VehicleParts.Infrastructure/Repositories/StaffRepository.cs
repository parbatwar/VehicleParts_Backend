using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class StaffRepository : IStaffRepository
{
    private readonly AppDbContext _context;

    public StaffRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _context.Staff
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task<Staff?> GetByIdAsync(int id)
    {
        return await _context.Staff
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Staff> CreateAsync(Staff staff)
    {
        _context.Staff.Add(staff);
        await _context.SaveChangesAsync();
        return staff;
    }

    public async Task UpdateAsync(Staff staff)
    {
        _context.Entry(staff).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetDistinctPositionsAsync()
    {
        return await _context.Staff
            .Select(s => s.Position)
            .Distinct()
            .OrderBy(p => p)
            .ToListAsync();
    }
}