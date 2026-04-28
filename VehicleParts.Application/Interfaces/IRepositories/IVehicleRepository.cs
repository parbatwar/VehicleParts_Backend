using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetByCustomerIdAsync(int customerId);
        Task<Vehicle?> GetByIdAsync(int id);
        Task<Vehicle> CreateAsync(Vehicle vehicle);
        Task<Vehicle> UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(Vehicle vehicle);
    }
}
