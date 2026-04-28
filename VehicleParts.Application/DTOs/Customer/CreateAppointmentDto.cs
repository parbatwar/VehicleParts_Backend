using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class CreateAppointmentDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
