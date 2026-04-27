using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Done,
    Cancelled
}

public class Appointment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    [Required]
    public int VehicleId { get; set; }

    [ForeignKey(nameof(VehicleId))]
    public Vehicle Vehicle { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}