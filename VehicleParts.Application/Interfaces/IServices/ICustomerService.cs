using VehicleParts.Application.DTOs.Customer;

namespace VehicleParts.Application.Interfaces.IServices;

public interface ICustomerService
{
    Task<CustomerResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerResponseDto>> SearchCustomersAsync(string searchTerm);
}
