using System;
using System.Collections.Generic;
using System.Text;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories
{
    public class CustomerInteractionRepository : ICustomerInteractionRepository
    {
        private readonly AppDbContext _context;

        public CustomerInteractionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<PartRequest> CreatePartRequestAsync(PartRequest request)
        {
            _context.PartRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }
    }
}
