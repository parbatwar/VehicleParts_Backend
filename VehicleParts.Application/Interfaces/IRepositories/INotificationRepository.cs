using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IEnumerable<Notification>> GetByUserIdAsync(long userId);
    Task<Notification?> GetByIdAsync(int id);
    Task UpdateAsync(Notification notification);
}