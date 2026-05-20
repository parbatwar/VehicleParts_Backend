using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface IPartRequestRepository
{
    Task<IEnumerable<PartRequest>> GetAllAsync();
    Task<PartRequest?> GetByIdAsync(int id);
    Task<PartRequest> UpdateAsync(PartRequest partRequest);
}