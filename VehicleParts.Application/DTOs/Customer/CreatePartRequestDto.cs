using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class CreatePartRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string PartName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
