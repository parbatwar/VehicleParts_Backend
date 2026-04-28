using VehicleParts.Application.DTOs.Part;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IPartService
{
    Task<IEnumerable<PartResponseDto>> GetAllAsync();
    Task<PartResponseDto> GetByIdAsync(int id);
    Task<PartResponseDto> CreateAsync(CreatePartDto dto);
    Task<PartResponseDto> UpdateAsync(int id, UpdatePartDto dto);
    Task DeleteAsync(int id);
}