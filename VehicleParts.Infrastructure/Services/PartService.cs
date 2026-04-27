using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Part;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class PartService : IPartService
{
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<PartService> _logger;

    public PartService(
        IPartRepository partRepository,
        IVendorRepository vendorRepository,
        ILogger<PartService> logger)
    {
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PartResponseDto>> GetAllAsync()
    {
        var parts = await _partRepository.GetAllAsync();
        return parts.Select(MapToResponse);
    }

    public async Task<PartResponseDto> GetByIdAsync(int id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null)
            throw new KeyNotFoundException($"Part with ID {id} not found.");

        return MapToResponse(part);
    }

    public async Task<PartResponseDto> CreateAsync(CreatePartDto dto)
    {
        // Check vendor exists
        var vendor = await _vendorRepository.GetByIdAsync(dto.VendorId);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {dto.VendorId} not found.");

        var part = new Part
        {
            VendorId = dto.VendorId,
            Name = dto.Name,
            PartNumber = dto.PartNumber,
            Category = dto.Category,
            UnitPrice = dto.UnitPrice,
            StockQty = dto.StockQty,
            ReorderLevel = dto.ReorderLevel,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _partRepository.CreateAsync(part);
        _logger.LogInformation("Part {Name} created successfully.", part.Name);

        var partWithVendor = await _partRepository.GetByIdAsync(created.Id);
        return MapToResponse(partWithVendor!);
    }

    public async Task<PartResponseDto> UpdateAsync(int id, UpdatePartDto dto)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null)
            throw new KeyNotFoundException($"Part with ID {id} not found.");

        // Check vendor exists if changed
        var vendor = await _vendorRepository.GetByIdAsync(dto.VendorId);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {dto.VendorId} not found.");

        part.Name = dto.Name;
        part.Category = dto.Category;
        part.UnitPrice = dto.UnitPrice;
        part.StockQty = dto.StockQty;
        part.ReorderLevel = dto.ReorderLevel;
        part.VendorId = dto.VendorId;
        part.UpdatedAt = DateTime.UtcNow;

        var updated = await _partRepository.UpdateAsync(part);
        _logger.LogInformation("Part {Id} updated successfully.", id);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(int id)
    {
        var part = await _partRepository.GetByIdAsync(id);
        if (part == null)
            throw new KeyNotFoundException($"Part with ID {id} not found.");

        await _partRepository.DeleteAsync(part);
        _logger.LogInformation("Part {Id} deleted successfully.", id);
    }

    private static PartResponseDto MapToResponse(Part part) => new()
    {
        Id = part.Id,
        Name = part.Name,
        PartNumber = part.PartNumber,
        Category = part.Category,
        UnitPrice = part.UnitPrice,
        StockQty = part.StockQty,
        ReorderLevel = part.ReorderLevel,
        VendorName = part.Vendor?.Name ?? string.Empty,
        UpdatedAt = part.UpdatedAt
    };
}