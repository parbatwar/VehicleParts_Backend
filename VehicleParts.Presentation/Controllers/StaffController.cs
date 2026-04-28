using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Staff;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IStaffService staffService, ILogger<StaffController> logger)
    {
        _staffService = staffService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staffList = await _staffService.GetAllAsync();
        return Ok(staffList);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetAvailableRoles()
    {
        var roles = await _staffService.GetAvailableRolesAsync();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var staff = await _staffService.CreateAsync(dto);
            return Ok(staff);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid staff creation request for {Email}.", dto.Email);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateStaffRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var staff = await _staffService.UpdateRoleAsync(id, dto);
            return Ok(staff);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Staff ID {StaffId} was not found for role update.", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid role update request for staff ID {StaffId}.", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
