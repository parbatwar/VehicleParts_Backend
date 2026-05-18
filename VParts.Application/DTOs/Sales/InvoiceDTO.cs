namespace VParts.Application.DTOs.Sales;

public class InvoiceDTO
{
    public int SaleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public List<SaleItemDTO> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
}