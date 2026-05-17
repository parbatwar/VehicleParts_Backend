using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPartRepository _partRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IPartRepository partRepository,
        INotificationService notificationService,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<SaleResponseDto> ProcessSaleAsync(CreateSaleDto dto)
    {
        var invoice = new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            StaffId = dto.StaffId,
            Date = DateTime.UtcNow,
            PaymentStatus = Enum.Parse<PaymentStatus>(dto.PaymentStatus, true),
            Items = new List<SalesItem>()
        };

        decimal subtotal = 0;

        foreach (var itemDto in dto.Items)
        {
            var part = await _partRepository.GetByIdAsync(itemDto.PartId);
            if (part == null)
                throw new KeyNotFoundException($"Part with ID {itemDto.PartId} not found.");

            if (part.StockQty < itemDto.Quantity)
                throw new ArgumentException(
                    $"Insufficient stock for {part.Name}. Available: {part.StockQty}, Requested: {itemDto.Quantity}");

            invoice.Items.Add(new SalesItem
            {
                PartId = part.Id,
                Quantity = itemDto.Quantity,
                Price = part.UnitPrice
            });

            // reduce stock
            part.StockQty -= itemDto.Quantity;
            part.UpdatedAt = DateTime.UtcNow;
            await _partRepository.UpdateAsync(part);

            // check low stock — notify admin
            if (part.StockQty <= part.ReorderLevel)
            {
                await _notificationService.CreateLowStockNotificationAsync(
                    part.Name, part.StockQty);
            }

            subtotal += itemDto.Quantity * part.UnitPrice;
        }

        // Feature 16 — 10% discount if subtotal > 5000
        decimal discount = 0;
        if (subtotal > 5000)
            discount = subtotal * 0.10m;

        invoice.SubTotal = subtotal;
        invoice.DiscountAmount = discount;
        invoice.TotalAmount = subtotal - discount;

        var created = await _saleRepository.CreateAsync(invoice);
        _logger.LogInformation(
            "Sale created. Customer: {CustomerId}, Total: {Total}, Discount: {Discount}",
            dto.CustomerId, invoice.TotalAmount, discount);

        var saleWithDetails = await _saleRepository.GetByIdAsync(created.Id);
        return MapToResponse(saleWithDetails!);
    }

    public async Task<SaleResponseDto?> GetSaleByIdAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null) return null;
        return MapToResponse(sale);
    }

    public async Task<List<SaleResponseDto>> GetAllSalesAsync()
    {
        var sales = await _saleRepository.GetAllAsync();
        return sales.Select(MapToResponse).ToList();
    }

    public async Task<List<SaleResponseDto>> GetSalesByCustomerAsync(int customerId)
    {
        var sales = await _saleRepository.GetByCustomerIdAsync(customerId);
        return sales.Select(MapToResponse).ToList();
    }

    private static SaleResponseDto MapToResponse(SalesInvoice sale) => new()
    {
        Id = sale.Id,
        CustomerId = sale.CustomerId,
        CustomerName = $"{sale.Customer.User.FirstName} {sale.Customer.User.LastName}",
        StaffId = sale.StaffId,
        StaffName = $"{sale.Staff.User.FirstName} {sale.Staff.User.LastName}",
        SubTotal = sale.SubTotal,
        DiscountAmount = sale.DiscountAmount,
        TotalAmount = sale.TotalAmount,
        DiscountApplied = sale.DiscountAmount > 0,
        PaymentStatus = sale.PaymentStatus.ToString(),
        Date = sale.Date,
        Items = sale.Items.Select(i => new SaleItemResponseDto
        {
            PartId = i.PartId,
            PartName = i.Part?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.Price,
            LineTotal = i.Quantity * i.Price
        }).ToList()
    };
}