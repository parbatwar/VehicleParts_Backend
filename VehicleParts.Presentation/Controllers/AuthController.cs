using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Auth;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find user
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        // Check password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return Unauthorized(new { message = "Invalid email or password." });

        // Get roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Generate token
        var token = _authService.GenerateToken(user, userRoles);

        _logger.LogInformation("User {Email} logged in successfully.", dto.Email);

        return Ok(new
        {
            token,
            email = user.Email,
            fullName = $"{user.FirstName} {user.LastName}",
            role = userRoles.FirstOrDefault()
        });
    }
}