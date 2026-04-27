using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum NotificationType
{
    LowStock,
    CreditReminder
}

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}