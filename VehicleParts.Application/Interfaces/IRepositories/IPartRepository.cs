using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface IPartRepository
{
    Task<IEnumerable<Part>> GetAllAsync();
    Task<Part?> GetByIdAsync(int id);
    Task<Part> CreateAsync(Part part);
    Task<Part> UpdateAsync(Part part);
    Task DeleteAsync(Part part);
}