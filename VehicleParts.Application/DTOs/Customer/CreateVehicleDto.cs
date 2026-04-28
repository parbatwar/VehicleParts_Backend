using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class CreateVehicleDto
    {
        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;
    }
}
