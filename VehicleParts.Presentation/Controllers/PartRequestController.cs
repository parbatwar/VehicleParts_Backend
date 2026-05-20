using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class PartRequestController : ControllerBase
{
    private readonly IPartRequestRepository _partRequestRepository;

    public PartRequestController(IPartRequestRepository partRequestRepository)
    {
        _partRequestRepository = partRequestRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _partRequestRepository.GetAllAsync();
        return Ok(requests.Select(r => new
        {
            r.Id,
            CustomerName = $"{r.Customer.User.FirstName} {r.Customer.User.LastName}",
            CustomerPhone = r.Customer.User.PhoneNumber,
            r.PartName,
            r.Description,
            r.Status,
            r.CreatedAt
        }));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePartRequestStatusDto dto)
    {
        var request = await _partRequestRepository.GetByIdAsync(id);
        if (request == null) return NotFound();

        request.Status = Enum.Parse<PartRequestStatus>(dto.Status, true);
        await _partRequestRepository.UpdateAsync(request);

        return Ok(new { message = "Status updated.", status = request.Status.ToString() });
    }
}

public class UpdatePartRequestStatusDto
{
    public string Status { get; set; } = string.Empty;
}