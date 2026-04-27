using Microsoft.AspNetCore.Identity;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly UserManager<User> _userManager;

    public CustomerService(ICustomerRepository customerRepository, UserManager<User> userManager)
    {
        _customerRepository = customerRepository;
        _userManager = userManager;
    }

    public async Task<CustomerResponseDto> RegisterCustomerAsync(RegisterCustomerDto dto)
    {
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            Phone = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Customer");

        var vehicle = new Vehicle
        {
            Brand = dto.Brand,
            Model = dto.Model,
            Year = dto.Year,
            PlateNumber = dto.PlateNumber
        };

        var customer = new Customer
        {
            UserId = user.Id,
            RegType = RegType.StaffRegistered,
            Vehicles = new List<Vehicle> { vehicle }
        };

        await _customerRepository.CreateAsync(customer);

        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = $"{user.FirstName} {user.LastName}",
            Email = user.Email!,
            Phone = user.Phone ?? "",
            CreditBalance = customer.CreditBalance,
            RegType = customer.RegType.ToString(),
            Vehicles = customer.Vehicles.Select(v => new VehicleDto
            {
                Id = v.Id,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year,
                PlateNumber = v.PlateNumber
            }).ToList()
        };
    }

    public async Task<CustomerResponseDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return null;
        return MapToDto(customer);
    }

    public async Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Select(MapToDto);
    }

    public async Task<IEnumerable<CustomerResponseDto>> SearchCustomersAsync(string searchTerm)
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.Where(c =>
            c.User.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.User.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.User.Email!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.User.Phone!.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.Vehicles.Any(v => v.PlateNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        ).Select(MapToDto);
    }

    private CustomerResponseDto MapToDto(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FullName = $"{customer.User.FirstName} {customer.User.LastName}",
            Email = customer.User.Email!,
            Phone = customer.User.Phone ?? "",
            CreditBalance = customer.CreditBalance,
            RegType = customer.RegType.ToString(),
            Vehicles = customer.Vehicles.Select(v => new VehicleDto
            {
                Id = v.Id,
                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year,
                PlateNumber = v.PlateNumber
            }).ToList()
        };
    }
}
