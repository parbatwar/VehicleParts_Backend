using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Notification;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        UserManager<User> userManager,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task CreateLowStockNotificationAsync(string partName, int currentStock)
    {
        // Find admin user
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins.FirstOrDefault();
        if (admin == null) return;

        var notification = new Notification
        {
            UserId = admin.Id,
            Type = NotificationType.LowStock,
            Message = $"Low stock alert: '{partName}' has only {currentStock} units remaining. Please restock soon.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(notification);
        _logger.LogInformation("Low stock notification created for part {PartName}.", partName);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetAdminNotificationsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins.FirstOrDefault();
        if (admin == null) return Enumerable.Empty<NotificationResponseDto>();

        var notifications = await _notificationRepository.GetByUserIdAsync(admin.Id);

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
            throw new KeyNotFoundException($"Notification {id} not found.");

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);
    }
}