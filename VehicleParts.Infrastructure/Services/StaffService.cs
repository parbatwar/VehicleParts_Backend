using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Staff;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository staffRepository,
        UserManager<User> userManager,
        ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<StaffResponseDto>> GetAllAsync()
    {
        var staffList = await _staffRepository.GetAllAsync();
        return staffList.Select(MapToResponse);
    }

    public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
    {
        // Check email not already taken
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ArgumentException("Email is already in use.");

        // Create user
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            NormalizedUserName = dto.Email.ToUpper(),
            PhoneNumber = dto.Phone,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Staff");

        // Create staff record
        var staff = new Staff
        {
            UserId = user.Id,
            Position = dto.Position
        };

        var created = await _staffRepository.CreateAsync(staff);
        _logger.LogInformation("Staff {Email} created successfully.", dto.Email);

        // Return with user info included
        var staffWithUser = await _staffRepository.GetAllAsync();
        var newStaff = staffWithUser.First(s => s.Id == created.Id);
        return MapToResponse(newStaff);
    }

    private static StaffResponseDto MapToResponse(Staff staff) => new()
    {
        Id = staff.Id,
        UserId = staff.UserId,
        FirstName = staff.User.FirstName,
        LastName = staff.User.LastName,
        Email = staff.User.Email!,
        Phone = staff.User.PhoneNumber ?? string.Empty,
        Position = staff.Position,
    };
}