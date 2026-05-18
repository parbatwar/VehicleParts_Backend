namespace VParts.Application.DTOs.Sales;

public class SaleDTO
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public List<SaleItemDTO> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public bool DiscountApplied { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
}