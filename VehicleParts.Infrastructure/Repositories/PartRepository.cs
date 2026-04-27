using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class PartRepository : IPartRepository
{
    private readonly AppDbContext _context;

    public PartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Part>> GetAllAsync()
    {
        return await _context.Parts
            .Include(p => p.Vendor)
            .ToListAsync();
    }

    public async Task<Part?> GetByIdAsync(int id)
    {
        return await _context.Parts
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Part> CreateAsync(Part part)
    {
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();
        return part;
    }

    public async Task<Part> UpdateAsync(Part part)
    {
        _context.Parts.Update(part);
        await _context.SaveChangesAsync();
        return part;
    }

    public async Task DeleteAsync(Part part)
    {
        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();
    }
}