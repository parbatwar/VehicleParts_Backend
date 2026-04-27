using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Part;

public class UpdatePartDto
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; }

    public int ReorderLevel { get; set; } = 10;

    public int VendorId { get; set; }
}