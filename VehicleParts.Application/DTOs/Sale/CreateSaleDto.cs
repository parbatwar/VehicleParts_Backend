using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Sale;

public class CreateSaleDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [Required]
    public List<SaleItemDto> Items { get; set; } = new();

    public string PaymentStatus { get; set; } = "paid";
}

public class SaleItemDto
{
    [Required]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}