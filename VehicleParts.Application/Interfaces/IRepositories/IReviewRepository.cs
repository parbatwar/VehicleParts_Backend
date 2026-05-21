using VehicleParts.Domain.Models;

namespace VehicleParts.Application.Interfaces.IRepositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateAsync(Review review);
        Task<Review?> GetByAppointmentAndCustomerAsync(int appointmentId, int customerId);
        Task<bool> ExistsByAppointmentAndCustomerAsync(int appointmentId, int customerId);
    }
}