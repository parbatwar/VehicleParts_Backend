using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly IVehicleRepository _vehicleRepo;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository customerRepo,
            IVehicleRepository vehicleRepo,
            UserManager<User> userManager,
            ILogger<CustomerService> logger)
        {
            _customerRepo = customerRepo;
            _vehicleRepo = vehicleRepo;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task RegisterSelfAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ArgumentException("Email is already in use.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                NormalizedEmail = dto.Email.ToUpper(),
                NormalizedUserName = dto.Email.ToUpper(),
                PhoneNumber = dto.Phone,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            var customer = new Customer
            {
                UserId = user.Id,
                RegType = RegType.SelfRegistered,
                CreditBalance = 0
            };

            await _customerRepo.CreateAsync(customer);
            _logger.LogInformation("Customer {Email} self-registered successfully.", dto.Email);
        }

        public async Task<CustomerProfileDto> GetProfileAsync(long userId)
        {
            var customer = await GetCustomerOrThrowAsync(userId);
            return MapToProfileDto(customer);
        }

        public async Task<CustomerProfileDto> UpdateProfileAsync(long userId, UpdateProfileDto dto)
        {
            var customer = await GetCustomerOrThrowAsync(userId);

            // Update user identity fields
            customer.User.FirstName = dto.FirstName;
            customer.User.LastName = dto.LastName;
            customer.User.PhoneNumber = dto.Phone;

            await _userManager.UpdateAsync(customer.User);

            return MapToProfileDto(customer);
        }

        public async Task<IEnumerable<VehicleDto>> GetVehiclesAsync(long userId)
        {
            var customer = await GetCustomerOrThrowAsync(userId);
            var vehicles = await _vehicleRepo.GetByCustomerIdAsync(customer.Id);
            return vehicles.Select(MapToVehicleDto);
        }

        public async Task<VehicleDto> AddVehicleAsync(long userId, CreateVehicleDto dto)
        {
            var customer = await GetCustomerOrThrowAsync(userId);

            var vehicle = new Vehicle
            {
                CustomerId = customer.Id,
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                PlateNumber = dto.PlateNumber
            };

            var created = await _vehicleRepo.CreateAsync(vehicle);
            return MapToVehicleDto(created);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(long userId, int vehicleId, CreateVehicleDto dto)
        {
            var customer = await GetCustomerOrThrowAsync(userId);
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId);

            if (vehicle == null || vehicle.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("Vehicle not found or access denied.");

            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.Year = dto.Year;
            vehicle.PlateNumber = dto.PlateNumber;

            var updated = await _vehicleRepo.UpdateAsync(vehicle);
            return MapToVehicleDto(updated);
        }

        public async Task DeleteVehicleAsync(long userId, int vehicleId)
        {
            var customer = await GetCustomerOrThrowAsync(userId);
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId);

            if (vehicle == null || vehicle.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("Vehicle not found or access denied.");

            await _vehicleRepo.DeleteAsync(vehicle);
        }

        // Helper methods
        private async Task<Customer> GetCustomerOrThrowAsync(long userId)
        {
            var customer = await _customerRepo.GetByUserIdAsync(userId);
            if (customer == null) throw new KeyNotFoundException("Customer profile not found.");
            return customer;
        }

        private static CustomerProfileDto MapToProfileDto(Customer customer) => new()
        {
            CustomerId = customer.Id,
            FirstName = customer.User.FirstName,
            LastName = customer.User.LastName,
            Email = customer.User.Email!,
            Phone = customer.User.PhoneNumber ?? string.Empty,
            CreditBalance = customer.CreditBalance
        };

        private static VehicleDto MapToVehicleDto(Vehicle vehicle) => new()
        {
            Id = vehicle.Id,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PlateNumber = vehicle.PlateNumber
        };
    }
}
