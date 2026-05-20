using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories;

public class PartRequestRepository : IPartRequestRepository
{
    private readonly AppDbContext _context;

    public PartRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PartRequest>> GetAllAsync()
    {
        return await _context.PartRequests
            .Include(p => p.Customer)
                .ThenInclude(c => c.User)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PartRequest?> GetByIdAsync(int id)
    {
        return await _context.PartRequests
            .Include(p => p.Customer)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PartRequest> UpdateAsync(PartRequest partRequest)
    {
        _context.PartRequests.Update(partRequest);
        await _context.SaveChangesAsync();
        return partRequest;
    }
}