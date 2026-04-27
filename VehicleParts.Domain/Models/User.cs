using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public class User : IdentityUser<long>
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
}