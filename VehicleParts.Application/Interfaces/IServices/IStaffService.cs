using VehicleParts.Application.DTOs.Staff;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IStaffService
{
    Task<IEnumerable<StaffResponseDto>> GetAllAsync();
    Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
    Task<StaffResponseDto> UpdateRoleAsync(int staffId, UpdateStaffRoleDto dto);
    Task<IEnumerable<string>> GetAvailableRolesAsync();
}
