using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Vendor;

public class UpdateVendorDto
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;
}