using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer?> GetByUserIdAsync(long userId);
    }
}
