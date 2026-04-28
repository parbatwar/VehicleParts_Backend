using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface IStaffRepository
{
    Task<IEnumerable<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int id);
    Task<Staff> CreateAsync(Staff staff);
    Task UpdateAsync(Staff staff);
    Task<IEnumerable<string>> GetDistinctPositionsAsync();
}
