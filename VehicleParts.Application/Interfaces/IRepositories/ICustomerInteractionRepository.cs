using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories
{
    public interface ICustomerInteractionRepository
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<PartRequest> CreatePartRequestAsync(PartRequest request);
        Task<Review> CreateReviewAsync(Review review);
    }
}
