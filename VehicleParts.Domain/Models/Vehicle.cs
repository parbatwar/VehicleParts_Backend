using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public class Vehicle
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Model { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    [Required]
    [MaxLength(20)]
    public string PlateNumber { get; set; } = string.Empty;

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}