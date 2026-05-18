using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetAll()
    {
        var sales = await _saleService.GetAllSalesAsync();
        return Ok(sales);
    }

    [HttpGet("customer/{customerId:int}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var sales = await _saleService.GetSalesByCustomerAsync(customerId);
        return Ok(sales);
    }

    [HttpPatch("{id:int}/mark-paid")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var sale = await _saleService.MarkAsPaidAsync(id);
        return Ok(sale);
    }

    // Feature 7 & 16: Create Sale + Invoice with 10% Discount 
    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _saleService.ProcessSaleAsync(dto);
        
        return CreatedAtAction(nameof(GetInvoice), new { id = result.Id }, result);
    }

    // Feature 7: Get Invoice
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var result = await _saleService.GetSaleByIdAsync(id);
        return result is null ? NotFound(new { message = $"Invoice {id} not found." }) : Ok(result);
    }

    // Feature 11: Send Invoice via Email (Gracefully handles SMTP throttling caps)
    [HttpPost("{id:int}/send-invoice")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> SendInvoice(int id)
    {
        // First check if the invoice even exists in the database
        var saleExists = await _saleService.GetSaleByIdAsync(id);
        if (saleExists is null) 
            return NotFound(new { message = $"Invoice {id} not found." });

        var sentResult = await _saleService.SendInvoiceEmailAsync(id);
        
        if (sentResult)
        {
            return Ok(new { message = "Invoice sent to customer email successfully." });
        }
        
        // Return 202 Accepted if database processing succeeded but external cloud email was throttled
        return Accepted(new { 
            message = "Invoice record validated inside ledger system. However, external email transmission was deferred because the daily SMTP server quota limit has been exceeded." 
        });
    }

    // Feature 9: Staff Analytics Reporting - Regulars
    [HttpGet("reports/regulars")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RegularCustomers([FromQuery] int minPurchases = 3)
    {
        var report = await _saleService.GetRegularCustomersAsync(minPurchases);
        return Ok(report);
    }

    // Feature 9: Staff Analytics Reporting - High Spenders
    [HttpGet("reports/high-spenders")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> HighSpenders([FromQuery] int topN = 10)
    {
        var report = await _saleService.GetHighSpendersAsync(topN);
        return Ok(report);
    }

    // Feature 9: Staff Analytics Reporting - Pending Credits
    [HttpGet("reports/pending-credits")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> PendingCredits()
    {
        var report = await _saleService.GetPendingCreditsAsync();
        return Ok(report);
    }

    // Khalti Payment Gateway Integration - Initiate
    [HttpPost("payments/khalti/initiate")]
    [Authorize(Roles = "Staff,Admin,Customer")]
    public async Task<IActionResult> KhaltiInitialize([FromBody] KhaltiInitiateRequestDto dto)
    {
        var result = await _saleService.InitiateKhaltiPaymentAsync(dto);
        return Ok(result);
    }
    
    // Khalti Payment Gateway Integration - Verify
    [HttpPost("payments/khalti/verify")]
    [Authorize(Roles = "Staff,Admin,Customer")]
    public async Task<IActionResult> KhaltiVerify([FromBody] KhaltiVerifyRequestDto dto)
    {
        var result = await _saleService.VerifyKhaltiPaymentAsync(dto);
        return Ok(result);
    }
}