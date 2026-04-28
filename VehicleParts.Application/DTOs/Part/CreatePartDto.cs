using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Part;

public class CreatePartDto
{
    [Required]
    public int VendorId { get; set; }

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
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    public int ReorderLevel { get; set; } = 10;
}