namespace VParts.Application.DTOs.Sales;

public class CreateSaleDTO
{
    public int CustomerId { get; set; }
    public List<SaleItemDTO> Items { get; set; } = new();
}