using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentController(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return Ok(appointments.Select(a => new
        {
            a.Id,
            CustomerName = $"{a.Customer.User.FirstName} {a.Customer.User.LastName}",
            CustomerPhone = a.Customer.User.PhoneNumber,
            Vehicle = a.Vehicle != null ? $"{a.Vehicle.Brand} {a.Vehicle.Model} ({a.Vehicle.PlateNumber})" : "N/A",
            a.Date,
            a.Status,
            a.Notes,
            a.CreatedAt
        }));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null) return NotFound();

        appointment.Status = Enum.Parse<AppointmentStatus>(dto.Status, true);
        await _appointmentRepository.UpdateAsync(appointment);

        return Ok(new { message = "Status updated.", status = appointment.Status.ToString() });
    }
}

public class UpdateStatusDto
{
    public string Status { get; set; } = string.Empty;
}