using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum PartRequestStatus
{
    Open,
    Fulfilled
}

public class PartRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string PartName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public PartRequestStatus Status { get; set; } = PartRequestStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}