using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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
    private readonly IStaffRepository _staffRepository;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IPartRepository partRepository,
        IStaffRepository staffRepository,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        IConfiguration config,
        IHttpClientFactory httpFactory,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _partRepository = partRepository;
        _staffRepository = staffRepository;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _config = config;
        _httpFactory = httpFactory;
        _logger = logger;
    }
    
    public async Task<SaleResponseDto> ProcessSaleAsync(CreateSaleDto dto)
    {
        var userEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userEmail))
            throw new UnauthorizedAccessException("User email not found in authorization token.");

        var staff = await _staffRepository.GetByEmailAsync(userEmail);
        if (staff == null)
            throw new KeyNotFoundException($"Staff record not found for email: {userEmail}");

        int staffId = staff.Id;

        if (!Enum.TryParse<PaymentStatus>(dto.PaymentStatus, true, out var parsedStatus))
        {
            parsedStatus = PaymentStatus.Paid;
        }

        var invoice = new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            StaffId = staffId, 
            PaymentStatus = parsedStatus,
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
                throw new ArgumentException($"Insufficient stock for {part.Name}. Available: {part.StockQty}, Requested: {itemDto.Quantity}");

            invoice.Items.Add(new SalesItem
            {
                PartId = part.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = part.UnitPrice 
            });

            part.StockQty -= itemDto.Quantity;
            part.UpdatedAt = DateTime.UtcNow;
            await _partRepository.UpdateAsync(part);

            if (part.StockQty <= part.ReorderLevel)
            {
                await _notificationService.CreateLowStockNotificationAsync(part.Name, part.StockQty);
            }

            subtotal += itemDto.Quantity * part.UnitPrice;
        }

        decimal discount = 0;
        if (subtotal > 5000)
            discount = Math.Round(subtotal * 0.10m, 2);

        invoice.SubTotal = subtotal;
        invoice.DiscountAmount = discount;
        invoice.TotalAmount = subtotal - discount;

        var created = await _saleRepository.CreateAsync(invoice);
        _logger.LogInformation("Sale processed. Customer: {CustomerId}, Staff: {StaffId}, Total: {Total}", dto.CustomerId, staffId, invoice.TotalAmount);

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
    
    public async Task<SaleResponseDto> MarkAsPaidAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId)
            ?? throw new KeyNotFoundException($"Invoice memory record {saleId} was not found.");

        sale.PaymentStatus = PaymentStatus.Paid;
        await _saleRepository.UpdateAsync(sale);

        return MapToResponse(sale);
    }

    // Feature 9: Analytical Reports 
    public Task<List<RegularCustomerReportDto>> GetRegularCustomersAsync(int minPurchases = 3)
        => _saleRepository.GetRegularCustomersAsync(minPurchases);

    public Task<List<HighSpenderReportDto>> GetHighSpendersAsync(int topN = 10)
        => _saleRepository.GetHighSpendersAsync(topN);

    public Task<List<PendingCreditReportDto>> GetPendingCreditsAsync()
        => _saleRepository.GetPendingCreditsAsync();

    // Email Transmission 
    public async Task<bool> SendInvoiceEmailAsync(int saleId)
    {
        var sale = await _saleRepository.GetByIdAsync(saleId);
        if (sale == null) return false;

        var customerEmail = sale.Customer.User.Email;
        var customerName = $"{sale.Customer.User.FirstName} {sale.Customer.User.LastName}";

        var itemsHtml = string.Join("", sale.Items.Select(i => $@"
        <tr>
            <td style='padding: 10px; border-bottom: 1px solid #eee;'>{i.Part?.Name}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{i.Quantity}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>Rs. {i.UnitPrice:N2}</td>
            <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>Rs. {(i.Quantity * i.UnitPrice):N2}</td>
        </tr>"));

        var discountRow = sale.DiscountAmount > 0 ? $@"
        <tr>
            <td colspan='3' style='padding: 8px 10px; text-align: right; color: #27ae60;'>Loyalty Discount (10%)</td>
            <td style='padding: 8px 10px; text-align: right; color: #27ae60;'>- Rs. {sale.DiscountAmount:N2}</td>
        </tr>" : "";

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
                    <tbody>{itemsHtml}</tbody>
                    <tfoot>
                        <tr>
                            <td colspan='3' style='padding: 8px 10px; text-align: right; color: #888;'>Subtotal</td>
                            <td style='padding: 8px 10px; text-align: right;'>Rs. {sale.SubTotal:N2}</td>
                        </tr>
                        {discountRow}
                        <tr style='border-top: 2px solid #1a1a2e;'>
                            <td colspan='3' style='padding: 12px 10px; text-align: right; font-weight: bold; font-size: 16px;'>TOTAL</td>
                            <td style='padding: 12px 10px; text-align: right; font-weight: bold; font-size: 16px; color: #f39c12;'>Rs. {sale.TotalAmount:N2}</td>
                        </tr>
                    </tfoot>
                </table>
            </div>
        </div>";

        try
        {
            // Executing MailKit via try-catch to absorb external provider limits safely
            await _emailService.SendEmailAsync(customerEmail!, customerName, subject, body);
            
            sale.EmailSent = true;
            await _saleRepository.UpdateAsync(sale);
            return true;
        }
        catch (Exception ex)
        {
            // Intercept quota limit exception and log it natively to Rider terminal console
            _logger.LogWarning("Email transmission throttled by external cloud provider. Error message: {Message}", ex.Message);
            
            // Return false to let the controller know it hit an external roadblock but saved internally
            return false;
        }
    }

    // Khalti Payment Gateway Integration 
    public async Task<KhaltiInitializeApiResponse> InitiateKhaltiPaymentAsync(KhaltiInitiateRequestDto dto)
    {
        var invoice = await _saleRepository.GetByIdAsync(dto.InvoiceId)
            ?? throw new KeyNotFoundException($"Invoice {dto.InvoiceId} not found.");

        var secretKey = _config["Khalti:SecretKey"]
            ?? throw new InvalidOperationException("Khalti:SecretKey configuration parameter missing.");

        long amountPaisa = (long)(invoice.TotalAmount * 100);

        var payload = new
        {
            return_url = dto.ReturnUrl,
            website_url = _config["Khalti:WebsiteUrl"] ?? "http://localhost:5173",
            amount = amountPaisa,
            purchase_order_id = $"INV-{invoice.Id}",
            purchase_order_name = $"GearUp Invoice #{invoice.Id}",
            customer_info = new
            {
                name = $"{invoice.Customer.User.FirstName} {invoice.Customer.User.LastName}",
                email = invoice.Customer.User.Email,
                phone = invoice.Customer.User.PhoneNumber
            }
        };

        var client = _httpFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://a.khalti.com/api/v2/epayment/initiate/");
        request.Headers.Add("Authorization", $"Key {secretKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Khalti integration request rejected: {body}");

        var result = JsonSerializer.Deserialize<KhaltiInitializeApiResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new Exception("Empty tracking response payload returned from Khalti.");

        invoice.KhaltiPidx = result.pidx;
        invoice.PaymentStatus = PaymentStatus.KhaltiPending;
        await _saleRepository.UpdateAsync(invoice);

        return result;
    }

    public async Task<KhaltiLookupApiResponse> VerifyKhaltiPaymentAsync(KhaltiVerifyRequestDto dto)
    {
        var secretKey = _config["Khalti:SecretKey"]
            ?? throw new InvalidOperationException("Khalti secret validation keys missing configuration setup.");

        var client = _httpFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://a.khalti.com/api/v2/epayment/lookup/");
        request.Headers.Add("Authorization", $"Key {secretKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(new { pidx = dto.Pidx }), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Khalti lookup evaluation verification failed: {body}");

        var result = JsonSerializer.Deserialize<KhaltiLookupApiResponse>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new Exception("Empty lookup response shape structure parsing evaluation data.");

        var invoice = await _saleRepository.GetByIdAsync(dto.InvoiceId);
        if (invoice is not null)
        {
            invoice.PaymentStatus = result.status == "Completed" ? PaymentStatus.KhaltiPaid : PaymentStatus.Overdue;
            await _saleRepository.UpdateAsync(invoice);
        }

        return result;
    }

    // Object Model Mapping Projections
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
            UnitPrice = i.UnitPrice,
            LineTotal = i.Quantity * i.UnitPrice
        }).ToList()
    };
}