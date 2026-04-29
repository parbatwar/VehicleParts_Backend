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

        //feature 14: Customer histry and purchase history
        Task<IEnumerable<Appointment>> GetCustomerAppointmentsAsync(int customerId);
        Task<IEnumerable<SalesInvoice>> GetCustomerPurchasesAsync(int customerId);
    }
}
