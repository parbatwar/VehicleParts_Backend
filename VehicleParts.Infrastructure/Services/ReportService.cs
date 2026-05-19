using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.DTOs.Report;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialReportDto> GetDailyReportAsync(DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1).AddSeconds(-1);
        return await GenerateReportAsync(startDate, endDate, "Daily");
    }

    public async Task<FinancialReportDto> GetMonthlyReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddSeconds(-1);
        return await GenerateReportAsync(startDate, endDate, "Monthly");
    }

    public async Task<FinancialReportDto> GetYearlyReportAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);
        return await GenerateReportAsync(startDate, endDate, "Yearly");
    }

    private async Task<FinancialReportDto> GenerateReportAsync(DateTime startDate, DateTime endDate, string reportType)
    {
        startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        // Get sales in date range
        var sales = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Include(s => s.Items)
                .ThenInclude(i => i.Part)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .ToListAsync();

        // Get purchases in date range
        var purchases = await _context.PurchaseInvoices
            .Include(p => p.Vendor)
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .ToListAsync();

        // Calculate totals
        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var totalExpense = purchases.Sum(p => p.TotalAmount);
        var netProfit = totalRevenue - totalExpense;

        return new FinancialReportDto
        {
            ReportType = reportType,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalRevenue = totalRevenue,
            TotalExpense = totalExpense,
            NetProfit = netProfit,
            Sales = sales.Select(s => new ReportSaleDto 
            {
                InvoiceId = s.Id,
                CustomerName = $"{s.Customer.User.FirstName} {s.Customer.User.LastName}",
                Amount = s.TotalAmount,
                Date = s.Date
            }).ToList(),
            Purchases = purchases.Select(p => new ReportPurchaseDto  
            {
                InvoiceId = p.Id,
                VendorName = p.Vendor?.Name ?? string.Empty,
                Amount = p.TotalAmount,
                Date = p.Date
            }).ToList()
        };
    }
}