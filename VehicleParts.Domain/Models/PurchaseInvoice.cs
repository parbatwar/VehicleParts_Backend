using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum InvoiceStatus
{
    Pending,
    Paid
}

public class PurchaseInvoice
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VendorId { get; set; }

    [ForeignKey(nameof(VendorId))]
    public Vendor Vendor { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public List<PurchaseItem> Items { get; set; } = new();
}