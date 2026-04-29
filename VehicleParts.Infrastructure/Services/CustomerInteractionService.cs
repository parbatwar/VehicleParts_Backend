using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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

            var review = new Review
            {
                CustomerId = customer.Id,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            return await _repository.CreateReviewAsync(review);
        }

        //feature 14
        public async Task<CustomerHistoryDto> GetCustomerHistoryAsync(long userId)
        {
            var customer = await GetCustomerByUserIdAsync(userId);

            var appointments = await _repository.GetCustomerAppointmentsAsync(customer.Id);
            var purchases = await _repository.GetCustomerPurchasesAsync(customer.Id);

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
                }).ToList()
            };
        }
    } 
} 