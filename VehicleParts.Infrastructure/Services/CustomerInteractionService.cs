using System;
using System.Collections.Generic;
using System.Text;
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
                Date = dto.Date,
                Notes = dto.Notes,
                
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

            var review = new Review
            {
                CustomerId = customer.Id,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateReviewAsync(review);
        }
    }
}
