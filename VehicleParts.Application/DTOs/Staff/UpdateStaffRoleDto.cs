using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Staff;

public class UpdateStaffRoleDto
{
    [Required]
    [MaxLength(50)]
    public string Position { get; set; } = string.Empty;
}