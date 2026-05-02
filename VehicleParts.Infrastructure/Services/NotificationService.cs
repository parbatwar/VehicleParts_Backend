using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleParts.Application.DTOs.Notification;
using VehicleParts.Application.Interfaces.IRepositories;
using VehicleParts.Application.Interfaces.IServices;
using VehicleParts.Domain.Models;
using VehicleParts.Infrastructure.Persistence;

namespace VehicleParts.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        UserManager<User> userManager,
        AppDbContext context,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task CreateLowStockNotificationAsync(string partName, int currentStock)
    {
        // Find admin user
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins.FirstOrDefault();
        if (admin == null) return;

        var notification = new Notification
        {
            UserId = admin.Id,
            Type = NotificationType.LowStock,
            Message = $"Low stock alert: '{partName}' has only {currentStock} units remaining. Please restock soon.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.CreateAsync(notification);
        _logger.LogInformation("Low stock notification created for part {PartName}.", partName);
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetAdminNotificationsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var admin = admins.FirstOrDefault();
        if (admin == null) return Enumerable.Empty<NotificationResponseDto>();

        var notifications = await _notificationRepository.GetByUserIdAsync(admin.Id);

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task MarkAsReadAsync(int id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
            throw new KeyNotFoundException($"Notification {id} not found.");

        notification.IsRead = true;
        await _notificationRepository.UpdateAsync(notification);
    }


    // Email

    public async Task SendCreditRemindersAsync()
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        // Find all overdue sales invoices older than 1 month
        var overdueInvoices = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Where(s => s.PaymentStatus == PaymentStatus.Overdue
                     && s.Date <= oneMonthAgo)
            .ToListAsync();

        foreach (var invoice in overdueInvoices)
        {
            var customer = invoice.Customer;
            var user = customer.User;

            // Send email
            var subject = "Payment Reminder — VehicleParts";
            var body = $@"
            <h2>Payment Reminder</h2>
            <p>Dear {user.FirstName} {user.LastName},</p>
            <p>This is a reminder that you have an outstanding balance 
               of <strong>Rs. {invoice.TotalAmount}</strong> 
               on Invoice #{invoice.Id} dated {invoice.Date:MMMM dd, yyyy}.</p>
            <p>Please clear your balance at your earliest convenience.</p>
            <br/>
            <p>Thank you,</p>
            <p>VehicleParts Team</p>
        ";

            await _emailService.SendEmailAsync(
                user.Email!,
                $"{user.FirstName} {user.LastName}",
                subject,
                body);

            // Also create notification record
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var admin = admins.FirstOrDefault();
            if (admin != null)
            {
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = admin.Id,
                    Type = NotificationType.CreditReminder,
                    Message = $"Credit reminder sent to {user.FirstName} {user.LastName} for Invoice #{invoice.Id} — Rs. {invoice.TotalAmount}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "Credit reminder sent to {Email} for Invoice #{InvoiceId}.",
                user.Email, invoice.Id);
        }
    }



}