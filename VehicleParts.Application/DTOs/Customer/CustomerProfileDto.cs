using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class CustomerProfileDto
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal CreditBalance { get; set; }
    }
}
