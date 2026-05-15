namespace VehicleParts.Application.DTOs.Reports;

public class CustomerReportDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal OutstandingCredit { get; set; }
    public decimal PendingCreditAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public bool HasOverdueCredit { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}

public class CustomerReportsSummaryDto
{
    public IEnumerable<CustomerReportDto> RegularCustomers { get; set; } = Array.Empty<CustomerReportDto>();
    public IEnumerable<CustomerReportDto> HighSpenders { get; set; } = Array.Empty<CustomerReportDto>();
    public IEnumerable<CustomerReportDto> PendingCredits { get; set; } = Array.Empty<CustomerReportDto>();
}
