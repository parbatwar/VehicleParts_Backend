using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public class Part
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VendorId { get; set; }

    [ForeignKey(nameof(VendorId))]
    public Vendor Vendor { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string PartNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    public int ReorderLevel { get; set; } = 10;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

    public ICollection<SalesItem> SalesItems { get; set; } = new List<SalesItem>();
}