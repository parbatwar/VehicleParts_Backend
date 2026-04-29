using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByUserIdAsync(long userId);
    Task<Customer?> GetByIdWithHistoryAsync(int id);
}
