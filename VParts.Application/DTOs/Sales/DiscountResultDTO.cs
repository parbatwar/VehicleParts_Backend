namespace VParts.Application.DTOs.Sales;

public class DiscountResultDTO
{
    public decimal OriginalAmount { get; set; }
    public bool DiscountApplied { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}