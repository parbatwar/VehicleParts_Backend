using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.DTOs.Part;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PartController : ControllerBase
{
    private readonly IPartService _partService;
    private readonly ILogger<PartController> _logger;

    public PartController(IPartService partService, ILogger<PartController> logger)
    {
        _partService = partService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var parts = await _partService.GetAllAsync();
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var part = await _partService.GetByIdAsync(id);
        return Ok(part);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var part = await _partService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePartDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var part = await _partService.UpdateAsync(id, dto);
        return Ok(part);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _partService.DeleteAsync(id);
        return NoContent();
    }
}