using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IPartRepository _partRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IPartRepository partRepository,
        IStaffRepository staffRepository,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
        _staffRepository = staffRepository;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SaleResponseDto> ProcessSaleAsync(CreateSaleDto dto)
    {
        // Get the logged-in user's email from the token
        var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userEmail))
            throw new UnauthorizedAccessException("User email not found in token");

        // Find staff by email
        var staff = await _staffRepository.GetByEmailAsync(userEmail);

        if (staff == null)
            throw new KeyNotFoundException($"Staff record not found for email: {userEmail}");

        // Now use this staffId
        int staffId = staff.Id;

        // Create invoice with staffId
        var invoice = new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            StaffId = staffId,  // From database, not from frontend
            PaymentStatus = Enum.Parse<PaymentStatus>(dto.PaymentStatus),
            Date = DateTime.UtcNow,
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
            "Sale created. Customer: {CustomerId}, Staff: {StaffId}, Total: {Total}, Discount: {Discount}",
            dto.CustomerId, staffId, invoice.TotalAmount, discount);

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

    public async Task SendInvoiceEmailAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null)
            throw new KeyNotFoundException($"Sale {saleId} not found.");

        var customerEmail = sale.Customer.User.Email;
        var customerName = $"{sale.Customer.User.FirstName} {sale.Customer.User.LastName}";

        var itemsHtml = string.Join("", sale.Items.Select(i => $@"
        <tr>
            <td style='padding: 10px; border-bottom: 1px solid #eee;'>{i.Part?.Name}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{i.Quantity}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>Rs. {i.Price.ToString("N2")}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>Rs. {(i.Quantity * i.Price)}</td>
        </tr>
    "));

        var discountRow = sale.DiscountAmount > 0 ? $@"
        <tr>
            <td colspan='3' style='padding: 8px 10px; text-align: right; color: #27ae60;'>Loyalty Discount (10%)</td>
            <td style='padding: 8px 10px; text-align: right; color: #27ae60;'>- Rs. {sale.DiscountAmount}</td>
        </tr>
    " : "";

        var subject = $"GearUp Invoice #{sale.Id}";
        var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 30px; text-align: center;'>
                <h1 style='color: #f39c12; margin: 0; font-size: 28px;'>GearUp</h1>
                <p style='color: #888; margin: 5px 0 0 0; font-size: 12px;'>Vehicle Parts System</p>
            </div>

            <div style='padding: 30px; background-color: #fff;'>
                <h2 style='color: #1a1a2e; margin: 0 0 5px 0;'>Invoice #{sale.Id}</h2>
                <p style='color: #888; font-size: 13px;'>Date: {sale.Date:MMMM dd, yyyy}</p>

                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0; font-size: 13px;'><strong>Customer:</strong> {customerName}</p>
                    <p style='margin: 5px 0 0 0; font-size: 13px;'><strong>Email:</strong> {customerEmail}</p>
                </div>

                <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
                    <thead>
                        <tr style='background-color: #1a1a2e;'>
                            <th style='padding: 12px 10px; text-align: left; color: #fff; font-size: 12px;'>Item</th>
                            <th style='padding: 12px 10px; text-align: center; color: #fff; font-size: 12px;'>Qty</th>
                            <th style='padding: 12px 10px; text-align: right; color: #fff; font-size: 12px;'>Unit Price</th>
                            <th style='padding: 12px 10px; text-align: right; color: #fff; font-size: 12px;'>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan='3' style='padding: 8px 10px; text-align: right; color: #888;'>Subtotal</td>
                            <td style='padding: 8px 10px; text-align: right;'>Rs. {sale.SubTotal}</td>
                        </tr>
                        {discountRow}
                        <tr style='border-top: 2px solid #1a1a2e;'>
                            <td colspan='3' style='padding: 12px 10px; text-align: right; font-weight: bold; font-size: 16px;'>TOTAL</td>
                            <td style='padding: 12px 10px; text-align: right; font-weight: bold; font-size: 16px; color: #f39c12;'>Rs. {sale.TotalAmount}</td>
                        </tr>
                    </tfoot>
                </table>

                <p style='color: #888; font-size: 12px; text-align: center; margin-top: 30px;'>
                    Thank you for your business!
                </p>
            </div>

            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <p style='color: #888; font-size: 11px; margin: 0;'>GearUp Vehicle Parts System</p>
            </div>
        </div>
    ";

        await _emailService.SendEmailAsync(customerEmail!, customerName, subject, body);

        // mark email as sent
        sale.EmailSent = true;
        await _saleRepository.UpdateAsync(sale);

        _logger.LogInformation("Invoice email sent for sale #{SaleId} to {Email}.", saleId, customerEmail);
    }

    public async Task<SaleResponseDto> MarkAsPaidAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null)
            throw new KeyNotFoundException($"Sale {saleId} not found.");

        if (sale.PaymentStatus == PaymentStatus.Paid)
            throw new ArgumentException("This invoice is already paid.");

        sale.PaymentStatus = PaymentStatus.Paid;
        await _saleRepository.UpdateAsync(sale);

        _logger.LogInformation("Sale #{SaleId} marked as paid.", saleId);
        return MapToResponse(sale);
    }

    private static SaleResponseDto MapToResponse(SalesInvoice sale) => new()
    {
        Id = sale.Id,
        CustomerId = sale.CustomerId,
        CustomerName = $"{sale.Customer.User.FirstName} {sale.Customer.User.LastName}",
        CustomerEmail = sale.Customer.User?.Email ?? string.Empty,
        CustomerPhone = sale.Customer.User?.PhoneNumber ?? string.Empty,
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