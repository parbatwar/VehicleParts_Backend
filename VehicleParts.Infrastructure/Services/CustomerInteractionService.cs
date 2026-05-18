using System;
using System.Linq;
using System.Threading.Tasks;
using VehicleParts.Application.DTOs.Customer;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;

namespace VehicleParts.Infrastructure.Services
{
    public class CustomerInteractionService : ICustomerInteractionService
    {
        private readonly ICustomerInteractionRepository _repository;
        private readonly ICustomerRepository _customerRepository;

        public CustomerInteractionService(
            ICustomerInteractionRepository repository,
            ICustomerRepository customerRepository)
        {
            _repository = repository;
            _customerRepository = customerRepository;
        }

        private async Task<Customer> GetCustomerByUserIdAsync(long userId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer == null)
                throw new UnauthorizedAccessException("Only registered customers can perform this action.");
            return customer;
        }

        public async Task<Appointment> BookAppointmentAsync(long userId, CreateAppointmentDto dto)
        {
            var customer = await GetCustomerByUserIdAsync(userId);

            var appointment = new Appointment
            {
                CustomerId = customer.Id,
                VehicleId = dto.VehicleId,
                Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
                Notes = dto.Notes ?? string.Empty,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateAppointmentAsync(appointment);
        }

        public async Task<PartRequest> RequestPartAsync(long userId, CreatePartRequestDto dto)
        {
            var customer = await GetCustomerByUserIdAsync(userId);

            var request = new PartRequest
            {
                CustomerId = customer.Id,
                PartName = dto.PartName,
                Description = dto.Description,
                Status = PartRequestStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreatePartRequestAsync(request);
        }

        public async Task<Review> SubmitReviewAsync(long userId, CreateReviewDto dto)
        {
            var customer = await GetCustomerByUserIdAsync(userId);

            // Validation to ensure customer has a completed service
            var hasCompletedService = await _repository.HasCompletedAppointmentAsync(customer.Id);
            if (!hasCompletedService)
            {
                throw new InvalidOperationException("You can only submit a review after completing at least one service appointment.");
            }

            var review = new Review
            {
                CustomerId = customer.Id,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateReviewAsync(review);
        }

        public async Task<CustomerHistoryDto> GetCustomerHistoryAsync(long userId)
        {
            var customer = await GetCustomerByUserIdAsync(userId);

            var appointments = await _repository.GetCustomerAppointmentsAsync(customer.Id);
            var purchases = await _repository.GetCustomerPurchasesAsync(customer.Id);

            // Fetch part requests from repository
            var partRequests = await _repository.GetCustomerPartRequestsAsync(customer.Id);

            return new CustomerHistoryDto
            {
                ServiceHistory = appointments.Select(a => new AppointmentHistoryDto
                {
                    Id = a.Id,
                    VehicleId = a.VehicleId,
                    Date = a.Date,
                    Notes = a.Notes ?? string.Empty,
                    Status = a.Status.ToString()
                }).ToList(),

                PurchaseHistory = purchases.Select(p => new PurchaseHistoryDto
                {
                    Id = p.Id,
                    TotalAmount = p.TotalAmount,
                    PaymentStatus = p.PaymentStatus.ToString(),
                    Date = p.Date
                }).ToList(),

                // Map part requests to the DTO
                PartRequestHistory = partRequests.Select(pr => new PartRequestHistoryDto
                {
                    Id = pr.Id,
                    PartName = pr.PartName,
                    Description = pr.Description,
                    Status = pr.Status.ToString(),
                    CreatedAt = pr.CreatedAt
                }).ToList()
            };
        }
    }
}