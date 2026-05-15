namespace VehicleParts.Application.DTOs.Notification;

public class NotificationResponseDto
{
    public int Id { get; set; }
    public long UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
