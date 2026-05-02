using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Report;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Services;

public class FinancialReportService : IFinancialReportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinancialReportService> _logger;

    public FinancialReportService(
        AppDbContext context,
        ILogger<FinancialReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FinancialReportDto> GenerateDailyReportAsync(DateTime date)
    {
        var start = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(date.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        return await GenerateReportAsync("Daily", start, end);
    }

    public async Task<FinancialReportDto> GenerateMonthlyReportAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(start.AddMonths(1).AddTicks(-1), DateTimeKind.Utc);
        return await GenerateReportAsync("Monthly", start, end);
    }

    public async Task<FinancialReportDto> GenerateYearlyReportAsync(int year)
    {
        var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        return await GenerateReportAsync("Yearly", start, end);
    }

    public Task SendEmailAsync(string toEmail, string toName, string subject, string body)
    {
        throw new NotImplementedException();
    }

    private async Task<FinancialReportDto> GenerateReportAsync(
        string reportType, DateTime start, DateTime end)
    {
        // Get sales in period
        var sales = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Where(s => s.Date >= start && s.Date <= end)
            .ToListAsync();

        // Get purchases in period
        var purchases = await _context.PurchaseInvoices
            .Include(p => p.Vendor)
            .Where(p => p.Date >= start && p.Date <= end)
            .ToListAsync();

        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var totalExpense = purchases.Sum(p => p.TotalAmount);
        var netProfit = totalRevenue - totalExpense;

        _logger.LogInformation(
            "{ReportType} report generated. Revenue: {Revenue}, Expense: {Expense}, Profit: {Profit}",
            reportType, totalRevenue, totalExpense, netProfit);

        return new FinancialReportDto
        {
            ReportType = reportType,
            PeriodStart = start,
            PeriodEnd = end,
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpense,
            NetProfit = netProfit,
            GeneratedAt = DateTime.UtcNow,
            Sales = sales.Select(s => new ReportSaleItemDto
            {
                InvoiceId = s.Id,
                CustomerName = $"{s.Customer.User.FirstName} {s.Customer.User.LastName}",
                Amount = s.TotalAmount,
                Date = s.Date
            }).ToList(),
            Purchases = purchases.Select(p => new ReportPurchaseItemDto
            {
                InvoiceId = p.Id,
                VendorName = p.Vendor?.Name ?? string.Empty,
                Amount = p.TotalAmount,
                Date = p.Date
            }).ToList()
        };
    }
}