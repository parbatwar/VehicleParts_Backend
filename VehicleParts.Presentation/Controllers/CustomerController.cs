using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
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

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {Id}", id);
            return StatusCode(500, "Internal Server Error");
        }
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

    [HttpGet("reports/regulars")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetRegularCustomersReport()
    {
        var result = await _customerService.GetRegularCustomersReportAsync();
        return Ok(result);
    }

    [HttpGet("reports/high-spenders")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetHighSpendersReport()
    {
        var result = await _customerService.GetHighSpendersReportAsync();
        return Ok(result);
    }

    [HttpGet("reports/pending-credits")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetPendingCreditsReport()
    {
        var result = await _customerService.GetPendingCreditsReportAsync();
        return Ok(result);
    }

    [HttpGet("reports/summary")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<IActionResult> GetCustomerReportsSummary()
    {
        var result = await _customerService.GetCustomerReportsSummaryAsync();
        return Ok(result);
    }

    [HttpPost("self-register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _customerService.RegisterSelfAsync(dto);
        return Ok(new { message = "Registration successful. Please login." });
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _customerService.GetProfileAsync(GetUserId());
        return Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _customerService.UpdateProfileAsync(GetUserId(), dto);
        return Ok(updated);
    }

    [HttpGet("vehicles")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetVehicles()
    {
        var vehicles = await _customerService.GetVehiclesAsync(GetUserId());
        return Ok(vehicles);
    }

    [HttpPost("vehicles")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vehicle = await _customerService.AddVehicleAsync(GetUserId(), dto);
        return Ok(vehicle);
    }

    [HttpPut("vehicles/{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vehicle = await _customerService.UpdateVehicleAsync(GetUserId(), id, dto);
        return Ok(vehicle);
    }

    [HttpDelete("vehicles/{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        await _customerService.DeleteVehicleAsync(GetUserId(), id);
        return NoContent();
    }

    private long GetUserId()
    {
        var idClaim = User.FindFirst("Id")?.Value;
        if (long.TryParse(idClaim, out long userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("Invalid token claims.");
    }
}
