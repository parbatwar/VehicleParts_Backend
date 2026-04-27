using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.DTOs.Customer;

namespace VehicleParts.Application.Interfaces.IServices
{
    public interface ICustomerService
    {
        // Registration
        Task RegisterSelfAsync(RegisterDto dto);

        // Profile Management
        Task<CustomerProfileDto> GetProfileAsync(long userId);
        Task<CustomerProfileDto> UpdateProfileAsync(long userId, UpdateProfileDto dto);

        // Vehicle Management
        Task<IEnumerable<VehicleDto>> GetVehiclesAsync(long userId);
        Task<VehicleDto> AddVehicleAsync(long userId, CreateVehicleDto dto);
        Task<VehicleDto> UpdateVehicleAsync(long userId, int vehicleId, CreateVehicleDto dto);
        Task DeleteVehicleAsync(long userId, int vehicleId);
    }
}
