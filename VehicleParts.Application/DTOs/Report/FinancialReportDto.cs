namespace VehicleParts.Application.DTOs.Report;

public class FinancialReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetProfit { get; set; }
    public List<ReportSaleDto> Sales { get; set; } = new();
    public List<ReportPurchaseDto> Purchases { get; set; } = new();
}

public class ReportSaleDto
{
    public int InvoiceId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class ReportPurchaseDto
{
    public int InvoiceId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}