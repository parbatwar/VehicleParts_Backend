using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.PurchaseInvoice;

public class CreatePurchaseInvoiceDto
{
    [Required]
    public int VendorId { get; set; }

    [Required]
    public List<CreatePurchaseItemDto> Items { get; set; } = new();
}

public class CreatePurchaseItemDto
{
    // if existing part
    public int? PartId { get; set; }

    // if new part
    public string? PartName { get; set; }
    public string? Category { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}