using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Staff;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class StaffService : IStaffService
{
    private static readonly string[] AllowedStaffRoles = ["Admin", "Staff"];

    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository staffRepository,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IEnumerable<StaffResponseDto>> GetAllAsync()
    {
        var staffList = await _staffRepository.GetAllAsync();
        var response = new List<StaffResponseDto>();

        foreach (var staff in staffList)
        {
            response.Add(await MapToResponseAsync(staff));
        }

        return response;
    }

    public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
    {
        var roleName = await ValidateRoleAsync(dto.Role);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ArgumentException("Email is already in use.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            Phone = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to create user: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to assign role: {errors}");
        }

        var staff = new Staff
        {
            UserId = user.Id,
            Position = dto.Position
        };

        var created = await _staffRepository.CreateAsync(staff);
        _logger.LogInformation("Staff {Email} created successfully with role {Role}.", dto.Email, roleName);

        var newStaff = await _staffRepository.GetByIdAsync(created.Id)
            ?? throw new KeyNotFoundException("Created staff record could not be found.");

        return await MapToResponseAsync(newStaff);
    }

    public async Task<StaffResponseDto> UpdateRoleAsync(int staffId, UpdateStaffRoleDto dto)
    {
        var roleName = await ValidateRoleAsync(dto.Role);
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new KeyNotFoundException("Staff not found.");

        var currentRoles = await _userManager.GetRolesAsync(staff.User);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(staff.User, currentRoles);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                throw new ArgumentException($"Failed to remove existing roles: {errors}");
            }
        }

        var addResult = await _userManager.AddToRoleAsync(staff.User, roleName);
        if (!addResult.Succeeded)
        {
            var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to assign new role: {errors}");
        }

        _logger.LogInformation("Staff ID {StaffId} role updated to {Role}.", staffId, roleName);

        return await MapToResponseAsync(staff);
    }

    public Task<IEnumerable<string>> GetAvailableRolesAsync()
    {
        var roles = AllowedStaffRoles
            .OrderBy(name => name)
            .AsEnumerable();

        return Task.FromResult(roles);
    }

    private async Task<string> ValidateRoleAsync(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role is required.");

        var role = await _roleManager.FindByNameAsync(roleName.Trim());
        if (role == null)
            throw new ArgumentException($"Role '{roleName}' does not exist.");

        if (!AllowedStaffRoles.Contains(role.Name!, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("Only Admin and Staff roles can be assigned through staff management.");

        return role.Name!;
    }

    private async Task<StaffResponseDto> MapToResponseAsync(Staff staff)
    {
        var roles = await _userManager.GetRolesAsync(staff.User);

        return new StaffResponseDto
        {
            Id = staff.Id,
            UserId = staff.UserId,
            FirstName = staff.User.FirstName,
            LastName = staff.User.LastName,
            Email = staff.User.Email!,
            Phone = staff.User.Phone ?? string.Empty,
            Position = staff.Position,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = true
        };
    }
}
