using Microsoft.EntityFrameworkCore;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Review> CreateAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review?> GetByAppointmentAndCustomerAsync(int appointmentId, int customerId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId && r.CustomerId == customerId);
        }

        public async Task<bool> ExistsByAppointmentAndCustomerAsync(int appointmentId, int customerId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.AppointmentId == appointmentId && r.CustomerId == customerId);
        }
    }
}