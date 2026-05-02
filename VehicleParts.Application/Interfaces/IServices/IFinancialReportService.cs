using VehicleParts.Application.DTOs.Report;

namespace VehicleParts.Application.Interfaces.IServices;

public interface IFinancialReportService
{
    Task<FinancialReportDto> GenerateDailyReportAsync(DateTime date);
    Task<FinancialReportDto> GenerateMonthlyReportAsync(int year, int month);
    Task<FinancialReportDto> GenerateYearlyReportAsync(int year);
}