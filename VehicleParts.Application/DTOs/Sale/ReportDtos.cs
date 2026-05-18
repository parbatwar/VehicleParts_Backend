namespace VehicleParts.Application.DTOs.Sale;

public class RegularCustomerReportDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public int TotalPurchases { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}

public class HighSpenderReportDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
}

public class PendingCreditReportDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public decimal CreditBalance { get; set; }
}