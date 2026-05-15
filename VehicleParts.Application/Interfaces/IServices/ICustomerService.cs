using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.DTOs.Reports;

namespace VehicleParts.Application.Interfaces.IServices;

public interface ICustomerService
{
    Task<CustomerResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task<CustomerResponseDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();
    Task<IEnumerable<CustomerResponseDto>> SearchCustomersAsync(string searchTerm);
    Task<CustomerResponseDto?> GetCustomerWithHistoryAsync(int id);
    Task DeleteCustomerAsync(int id);
    Task<IEnumerable<CustomerReportDto>> GetRegularCustomersReportAsync();
    Task<IEnumerable<CustomerReportDto>> GetHighSpendersReportAsync();
    Task<IEnumerable<CustomerReportDto>> GetPendingCreditsReportAsync();
    Task<CustomerReportsSummaryDto> GetCustomerReportsSummaryAsync();
    Task RegisterSelfAsync(RegisterDto dto);
    Task<CustomerProfileDto> GetProfileAsync(long userId);
    Task<CustomerProfileDto> UpdateProfileAsync(long userId, UpdateProfileDto dto);
    Task<IEnumerable<VehicleDto>> GetVehiclesAsync(long userId);
    Task<VehicleDto> AddVehicleAsync(long userId, CreateVehicleDto dto);
    Task<VehicleDto> UpdateVehicleAsync(long userId, int vehicleId, CreateVehicleDto dto);
    Task DeleteVehicleAsync(long userId, int vehicleId);
}
