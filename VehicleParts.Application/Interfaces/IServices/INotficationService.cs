using VehicleParts.Application.DTOs.Notification;

namespace VehicleParts.Application.Interfaces.IServices;

public interface INotificationService
{
    Task CreateLowStockNotificationAsync(string partName, int currentStock);
    Task<IEnumerable<NotificationResponseDto>> GetAdminNotificationsAsync();
    Task MarkAsReadAsync(int id);
    Task SendCreditRemindersAsync();
}