using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Application.DTOs.Auth;

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

    // Komal's Part

    // 1. Self Registration (Open Endpoint)
    [HttpPost("self-register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _customerService.RegisterSelfAsync(dto);
        return Ok(new { message = "Registration successful. Please login." });
    }

    // 2. Get Profile
    [HttpGet("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _customerService.GetProfileAsync(GetUserId());
        return Ok(profile);
    }

    // 3. Update Profile
    [HttpPut("profile")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var updated = await _customerService.UpdateProfileAsync(GetUserId(), dto);
        return Ok(updated);
    }

    // 4. Get Vehicles
    [HttpGet("vehicles")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetVehicles()
    {
        var vehicles = await _customerService.GetVehiclesAsync(GetUserId());
        return Ok(vehicles);
    }

    // 5. Add Vehicle
    [HttpPost("vehicles")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vehicle = await _customerService.AddVehicleAsync(GetUserId(), dto);
        return Ok(vehicle);
    }

    // 6. Update Vehicle
    [HttpPut("vehicles/{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vehicle = await _customerService.UpdateVehicleAsync(GetUserId(), id, dto);
        return Ok(vehicle);
    }

    // 7. Delete Vehicle
    [HttpDelete("vehicles/{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        await _customerService.DeleteVehicleAsync(GetUserId(), id);
        return NoContent();
    }



    // Helper to extract User ID from JWT Token Claims
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