using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Vendor;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<VendorService> _logger;

    public VendorService(IVendorRepository vendorRepository, ILogger<VendorService> logger)
    {
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VendorResponseDto>> GetAllAsync()
    {
        var vendors = await _vendorRepository.GetAllAsync();
        return vendors.Select(MapToResponse);
    }

    public async Task<VendorResponseDto> GetByIdAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        return MapToResponse(vendor);
    }

    public async Task<VendorResponseDto> CreateAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _vendorRepository.CreateAsync(vendor);
        _logger.LogInformation("Vendor {Name} created successfully.", vendor.Name);
        return MapToResponse(created);
    }

    public async Task<VendorResponseDto> UpdateAsync(int id, UpdateVendorDto dto)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        vendor.Name = dto.Name;
        vendor.Phone = dto.Phone;
        vendor.Email = dto.Email;
        vendor.Address = dto.Address;

        var updated = await _vendorRepository.UpdateAsync(vendor);
        _logger.LogInformation("Vendor {Id} updated successfully.", id);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var vendor = await _vendorRepository.GetByIdAsync(id);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        await _vendorRepository.DeleteAsync(vendor);
        _logger.LogInformation("Vendor {Id} deleted successfully.", id);
    }

    private static VendorResponseDto MapToResponse(Vendor vendor) => new()
    {
        Id = vendor.Id,
        Name = vendor.Name,
        Phone = vendor.Phone,
        Email = vendor.Email,
        Address = vendor.Address,
        CreatedAt = vendor.CreatedAt
    };
}