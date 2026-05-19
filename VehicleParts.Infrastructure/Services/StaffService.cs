using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IEmailService _emailService;
    private readonly ILogger<StaffService> _logger;

    public StaffService(
        IStaffRepository staffRepository,
        UserManager<User> userManager,
        IEmailService emailService,
        ILogger<StaffService> logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
        _emailService = emailService;
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
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ArgumentException("Email is already in use.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.Phone
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Staff");

        var staff = new Staff
        {
            UserId = user.Id,
            Position = dto.Position
        };

        var created = await _staffRepository.CreateAsync(staff);
        _logger.LogInformation("Staff {Email} created as {Position}.", dto.Email, dto.Position);

        // Send credentials email
        var subject = "Welcome to GearUp — Your Account Details";
        var body = $@"
            <h2>Welcome to GearUp System</h2>
            <p>Dear {dto.FirstName} {dto.LastName},</p>
            <p>Your staff account has been created successfully. 
               Here are your login credentials:</p>
            <table style='border-collapse: collapse; width: 300px;'>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd;'><strong>Email</strong></td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{dto.Email}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd;'><strong>Password</strong></td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{dto.Password}</td>
                </tr>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd;'><strong>Position</strong></td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>{dto.Position}</td>
                </tr>
            </table>
            <br/>
            <p>Please login at <a href='http://localhost:5173'>GearUp System</a></p>
            <p>We recommend changing your password after first login.</p>
            <br/>
            <p>Regards,</p>
            <p>GearUp Admin</p>
        ";

        await _emailService.SendEmailAsync(
            dto.Email,
            $"{dto.FirstName} {dto.LastName}",
            subject,
            body);

        _logger.LogInformation("Credentials email sent to {Email}.", dto.Email);

        // return at the end
        var staffWithUser = await _staffRepository.GetByIdAsync(created.Id);
        return await MapToResponseAsync(staffWithUser!);
    }

    public async Task<StaffResponseDto> UpdateRoleAsync(int staffId, UpdateStaffRoleDto dto)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new KeyNotFoundException("Staff not found.");

        // We update ONLY the position text. We do NOT touch Identity roles here.
        staff.Position = dto.Position;

        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff ID {StaffId} updated to position: {Position}.", staffId, dto.Position);

        return await MapToResponseAsync(staff);
    }

    public async Task<IEnumerable<string>> GetAvailableRolesAsync()
    {
        // Ask the repository for the data instead of trying to use _context
        var rolesInUse = await _staffRepository.GetDistinctPositionsAsync();

        return rolesInUse;
    }

    private async Task<StaffResponseDto> MapToResponseAsync(Staff staff)
    {
        // We still fetch roles for the response, but we don't use them for the "Position" field logic anymore.
        return new StaffResponseDto
        {
            Id = staff.Id,
            UserId = staff.UserId,
            FirstName = staff.User.FirstName,
            LastName = staff.User.LastName,
            Email = staff.User.Email!,
            Phone = staff.User.PhoneNumber ?? string.Empty,
            Position = staff.Position,
            IsActive = true
        };
    }

    public async Task<StaffResponseDto> UpdateStaffAsync(int staffId, UpdateStaffDto dto)
    {
        // Fetch staff including the User object
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new KeyNotFoundException("Staff member not found.");

        // 1. Update Identity User fields
        staff.User.FirstName = dto.FirstName;
        staff.User.LastName = dto.LastName;
        staff.User.Email = dto.Email;
        staff.User.UserName = dto.Email; // Keep username synced with email
        staff.User.PhoneNumber = dto.Phone;

        var userResult = await _userManager.UpdateAsync(staff.User);
        if (!userResult.Succeeded)
        {
            var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
            throw new ArgumentException($"Failed to update identity user: {errors}");
        }

        // 2. Update Staff-specific fields
        staff.Position = dto.Position;
        await _staffRepository.UpdateAsync(staff);

        _logger.LogInformation("Staff ID {StaffId} updated successfully.", staffId);

        return await MapToResponseAsync(staff);
    }

    public async Task DeleteStaffAsync(int staffId)
    {
        var staff = await _staffRepository.GetByIdAsync(staffId)
            ?? throw new KeyNotFoundException("Staff record not found.");

        // 1. Remove the record from the Staff table
        await _staffRepository.DeleteAsync(staff);

        // 2. Remove the record from the AspNetUsers table
        if (staff.User != null)
        {
            var result = await _userManager.DeleteAsync(staff.User);
            if (!result.Succeeded)
            {
                throw new Exception("Staff record removed, but failed to delete login account.");
            }
        }

        _logger.LogWarning("Staff ID {StaffId} and associated User account permanently deleted.", staffId);
    }

    public async Task<StaffResponseDto?> GetByUserIdAsync(long userId)
    {
        var staff = await _staffRepository.GetByUserIdAsync(userId);
        if (staff == null) return null;
        return await MapToResponseAsync(staff);
    }
}