using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class CustomerInteractionController : ControllerBase
{
    private readonly ICustomerInteractionService _interactionService;
    private readonly ICustomerService _customerService;

    public CustomerInteractionController(
            ICustomerInteractionService interactionService,
            ICustomerService customerService)
    {
        _interactionService = interactionService;
        _customerService = customerService;
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("Id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException("ID Claim missing");
        return long.Parse(userIdClaim);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var userId = GetCurrentUserId();
            var appointment = await _interactionService.BookAppointmentAsync(userId, dto);
            return Ok(new { message = "Appointment booked successfully.", appointmentId = appointment.Id });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("part-requests")]
    public async Task<IActionResult> RequestPart([FromBody] CreatePartRequestDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var userId = GetCurrentUserId();
            var request = await _interactionService.RequestPartAsync(userId, dto);
            return Ok(new { message = "Part requested successfully.", requestId = request.Id });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("reviews")]
    public async Task<IActionResult> SubmitReview([FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var userId = GetCurrentUserId();
            var review = await _interactionService.SubmitReviewAsync(userId, dto);
            return Ok(new { message = "Review submitted successfully.", reviewId = review.Id });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    
    [HttpGet("history")]
    public async Task<IActionResult> GetMyHistory()
    {
            
        try
        {
            var userId = GetCurrentUserId();
            var history = await _interactionService.GetCustomerHistoryAsync(userId);
            return Ok(history);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

}


