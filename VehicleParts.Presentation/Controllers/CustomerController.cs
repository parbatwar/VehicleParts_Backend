using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost("register")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto dto)
    {
        var result = await _customerService.RegisterCustomerAsync(dto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetAllCustomers()
    {
        var result = await _customerService.GetAllCustomersAsync();
        return Ok(result);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string searchTerm)
    {
        var result = await _customerService.SearchCustomersAsync(searchTerm);
        return Ok(result);
    }
    
    [HttpGet("{id}/history")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetCustomerWithHistory(int id)
    {
        var result = await _customerService.GetCustomerWithHistoryAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
