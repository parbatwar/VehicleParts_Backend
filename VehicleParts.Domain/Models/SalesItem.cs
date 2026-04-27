using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public class SalesItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public SalesInvoice Invoice { get; set; } = null!;

    [Required]
    public int PartId { get; set; }

    [ForeignKey(nameof(PartId))]
    public Part Part { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}