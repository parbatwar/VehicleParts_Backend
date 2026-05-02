using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.PurchaseInvoice;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PurchaseInvoiceController : ControllerBase
{
    private readonly IPurchaseInvoiceService _invoiceService;
    private readonly ILogger<PurchaseInvoiceController> _logger;

    public PurchaseInvoiceController(
        IPurchaseInvoiceService invoiceService,
        ILogger<PurchaseInvoiceController> logger)
    {
        _invoiceService = invoiceService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var invoices = await _invoiceService.GetAllAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var invoice = await _invoiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }
}