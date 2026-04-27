namespace VehicleParts.Application.DTOs.Customer;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public string RegType { get; set; } = string.Empty;
    public List<VehicleDto> Vehicles { get; set; } = new();
    public List<PurchaseHistoryDto> PurchaseHistory { get; set; } = new();
}

public class PurchaseHistoryDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}