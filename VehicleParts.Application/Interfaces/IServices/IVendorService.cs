using VehicleParts.Application.DTOs.Vendor;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IVendorService
{
    Task<IEnumerable<VendorResponseDto>> GetAllAsync();
    Task<VendorResponseDto> GetByIdAsync(int id);
    Task<VendorResponseDto> CreateAsync(CreateVendorDto dto);
    Task<VendorResponseDto> UpdateAsync(int id, UpdateVendorDto dto);
    Task DeleteAsync(int id);
}