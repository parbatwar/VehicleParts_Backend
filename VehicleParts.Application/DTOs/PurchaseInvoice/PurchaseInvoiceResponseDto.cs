namespace VehicleParts.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceResponseDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<PurchaseItemResponseDto> Items { get; set; } = new();
}

public class PurchaseItemResponseDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}