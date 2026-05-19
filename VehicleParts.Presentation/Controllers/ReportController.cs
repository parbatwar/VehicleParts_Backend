using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateTime date)
    {
        var report = await _reportService.GetDailyReportAsync(date);
        return Ok(report);
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        var report = await _reportService.GetMonthlyReportAsync(year, month);
        return Ok(report);
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
    {
        var report = await _reportService.GetYearlyReportAsync(year);
        return Ok(report);
    }
}