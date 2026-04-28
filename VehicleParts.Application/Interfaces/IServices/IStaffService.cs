using VehicleParts.Application.DTOs.Staff;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IStaffService
{
    Task<IEnumerable<StaffResponseDto>> GetAllAsync();
    Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
    Task<StaffResponseDto> UpdateRoleAsync(int staffId, UpdateStaffRoleDto dto);
    Task<StaffResponseDto> UpdateStaffAsync(int staffId, UpdateStaffDto dto);
    Task DeleteStaffAsync(int staffId);
    Task<IEnumerable<string>> GetAvailableRolesAsync();
}
