using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.PurchaseInvoice;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IPurchaseInvoiceRepository _invoiceRepository;
    private readonly IPartRepository _partRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ILogger<PurchaseInvoiceService> _logger;

    public PurchaseInvoiceService(
        IPurchaseInvoiceRepository invoiceRepository,
        IPartRepository partRepository,
        IVendorRepository vendorRepository,
        ILogger<PurchaseInvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _partRepository = partRepository;
        _vendorRepository = vendorRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PurchaseInvoiceResponseDto>> GetAllAsync()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        return invoices.Select(MapToResponse);
    }

    public async Task<PurchaseInvoiceResponseDto> GetByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        if (invoice == null)
            throw new KeyNotFoundException($"Purchase invoice with ID {id} not found.");

        return MapToResponse(invoice);
    }

    public async Task<PurchaseInvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto)
    {
        var vendor = await _vendorRepository.GetByIdAsync(dto.VendorId);
        if (vendor == null)
            throw new KeyNotFoundException($"Vendor with ID {dto.VendorId} not found.");

        var invoice = new PurchaseInvoice
        {
            VendorId = dto.VendorId,
            Date = DateTime.UtcNow,
            Status = InvoiceStatus.Paid,
            Items = new List<PurchaseItem>()
        };

        decimal total = 0;

        foreach (var itemDto in dto.Items)
        {
            Part part;

            if (itemDto.PartId.HasValue)
            {
                // existing part — just update stock
                part = await _partRepository.GetByIdAsync(itemDto.PartId.Value)
                    ?? throw new KeyNotFoundException($"Part {itemDto.PartId} not found.");

                part.StockQty += itemDto.Quantity;
                part.UnitPrice = itemDto.UnitPrice;
                part.UpdatedAt = DateTime.UtcNow;
                await _partRepository.UpdateAsync(part);
            }
            else
            {
                // new part — create it
                part = new Part
                {
                    VendorId = dto.VendorId,
                    Name = itemDto.PartName!,
                    Category = itemDto.Category!,
                    UnitPrice = itemDto.UnitPrice,
                    StockQty = itemDto.Quantity,
                    ReorderLevel = 10,
                    UpdatedAt = DateTime.UtcNow
                };
                await _partRepository.CreateAsync(part);
            }

            invoice.Items.Add(new PurchaseItem
            {
                PartId = part.Id,
                Quantity = itemDto.Quantity,
                Price = itemDto.UnitPrice
            });

            total += itemDto.Quantity * itemDto.UnitPrice;
        }

        invoice.TotalAmount = total;
        var created = await _invoiceRepository.CreateAsync(invoice);
        _logger.LogInformation("Purchase invoice created for vendor {VendorId}.", dto.VendorId);

        var invoiceWithDetails = await _invoiceRepository.GetByIdAsync(created.Id);
        return MapToResponse(invoiceWithDetails!);
    }

    private static PurchaseInvoiceResponseDto MapToResponse(PurchaseInvoice invoice) => new()
    {
        Id = invoice.Id,
        VendorId = invoice.VendorId,
        VendorName = invoice.Vendor?.Name ?? string.Empty,
        TotalAmount = invoice.TotalAmount,
        Status = invoice.Status.ToString(),
        Date = invoice.Date,
        Items = invoice.Items.Select(i => new PurchaseItemResponseDto
        {
            PartId = i.PartId,
            PartName = i.Part?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.Price,
            Total = i.Quantity * i.Price
        }).ToList()
    };
}