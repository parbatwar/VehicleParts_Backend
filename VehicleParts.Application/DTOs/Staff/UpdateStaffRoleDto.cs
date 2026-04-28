using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Staff;

public class UpdateStaffRoleDto
{
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = string.Empty;
}
