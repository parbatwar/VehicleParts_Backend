namespace VehicleParts.Application.DTOs.Part;

public class PartResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQty { get; set; }
    public int ReorderLevel { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}