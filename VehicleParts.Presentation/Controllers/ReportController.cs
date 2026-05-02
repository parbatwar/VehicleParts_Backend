using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IFinancialReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IFinancialReportService reportService,
        ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var report = await _reportService.GenerateDailyReportAsync(date);
        return Ok(report);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (month < 1 || month > 12)
            return BadRequest(new { message = "Month must be between 1 and 12." });

        var report = await _reportService.GenerateMonthlyReportAsync(year, month);
        return Ok(report);
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
    {
        var report = await _reportService.GenerateYearlyReportAsync(year);
        return Ok(report);
    }
}