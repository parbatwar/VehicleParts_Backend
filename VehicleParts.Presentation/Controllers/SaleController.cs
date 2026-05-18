using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Sale;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaleController : ControllerBase
{
    private readonly ISaleService _saleService;
    private readonly ILogger<SaleController> _logger;

    public SaleController(ISaleService saleService, ILogger<SaleController> logger)
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

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetById(int id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        if (sale == null) return NotFound();
        return Ok(sale);
    }

    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var sales = await _saleService.GetSalesByCustomerAsync(customerId);
        return Ok(sales);
    }

    [HttpPost]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> ProcessSale([FromBody] CreateSaleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var sale = await _saleService.ProcessSaleAsync(dto);
        return Ok(sale);
    }

    [HttpPost("{id}/send-invoice")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> SendInvoice(int id)
    {
        await _saleService.SendInvoiceEmailAsync(id);
        return Ok(new { message = "Invoice sent successfully." });
    }

    [HttpPatch("{id}/mark-paid")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var sale = await _saleService.MarkAsPaidAsync(id);
        return Ok(sale);
    }
}