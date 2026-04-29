using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleParts.Application.DTOs.Customer
{
    public class CustomerHistoryDto
    {
        public List<AppointmentHistoryDto> ServiceHistory { get; set; } = new();
        public List<PurchaseHistoryDto> PurchaseHistory { get; set; } = new();
    }

    public class AppointmentHistoryDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PurchaseHistoryDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
