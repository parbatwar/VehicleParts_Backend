using VehicleParts.Application.DTOs.Staff;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IStaffService
{
    Task<IEnumerable<StaffResponseDto>> GetAllAsync();
    Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
}