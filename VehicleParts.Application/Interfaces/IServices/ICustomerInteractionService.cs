using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IServices
{
    public interface ICustomerInteractionService
    {
        Task<Appointment> BookAppointmentAsync(long userId, CreateAppointmentDto dto);
        Task<PartRequest> RequestPartAsync(long userId, CreatePartRequestDto dto);
        Task<Review> SubmitReviewAsync(long userId, CreateReviewDto dto);
    }
}
