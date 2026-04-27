using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum RegType
{
    SelfRegistered,
    StaffRegistered
}

public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditBalance { get; set; } = 0;

    public RegType RegType { get; set; } = RegType.SelfRegistered;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
