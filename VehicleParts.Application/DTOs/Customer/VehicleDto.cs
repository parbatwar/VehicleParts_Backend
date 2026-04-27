using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
    }
}
