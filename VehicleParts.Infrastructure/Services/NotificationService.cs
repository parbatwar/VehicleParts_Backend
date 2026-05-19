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

        // 1. First, mark Credit invoices older than 1 month as Overdue
        var creditInvoices = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Where(s => s.PaymentStatus == PaymentStatus.Credit
                        && s.Date <= oneMonthAgo)
            .ToListAsync();

        foreach (var invoice in creditInvoices)
        {
            invoice.PaymentStatus = PaymentStatus.Overdue;
            _logger.LogInformation("Invoice #{InvoiceId} marked as Overdue.", invoice.Id);
        }
        await _context.SaveChangesAsync();

        // 2. Now get all Overdue invoices (including newly marked ones)
        var overdueInvoices = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Where(s => s.PaymentStatus == PaymentStatus.Overdue)
            .ToListAsync();

        foreach (var invoice in overdueInvoices)
        {
            var customer = invoice.Customer;
            var user = customer.User;

            // Skip if no email
            if (string.IsNullOrEmpty(user.Email)) continue;

            // Optional: Check if reminder already sent in last 30 days
            var recentReminder = await _context.Notifications
                .Where(n => n.Type == NotificationType.CreditReminder
                            && n.Message.Contains($"Invoice #{invoice.Id}")
                            && n.CreatedAt > DateTime.UtcNow.AddDays(-30))
                .AnyAsync();

            if (recentReminder) continue;

            // Send email
            var subject = "Payment Reminder — GearUp Vehicle Parts";
            var body = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <div style='background-color: #1a1a2e; padding: 30px; text-align: center;'>
                <h1 style='color: #f39c12; margin: 0;'>GearUp</h1>
                <p style='color: #888; margin: 5px 0 0 0;'>Vehicle Parts System</p>
            </div>
            
            <div style='padding: 30px; background-color: #fff;'>
                <h2 style='color: #e74c3c; margin: 0 0 10px 0;'>⚠️ Payment Overdue</h2>
                <p>Dear {user.FirstName} {user.LastName},</p>
                <p>Your payment is now <strong>overdue</strong>. Please settle the outstanding balance at your earliest convenience.</p>
                
                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                    <p style='margin: 0;'><strong>Invoice #:</strong> {invoice.Id}</p>
                    <p style='margin: 5px 0 0 0;'><strong>Date:</strong> {invoice.Date:MMMM dd, yyyy}</p>
                    <p style='margin: 5px 0 0 0;'><strong>Amount Due:</strong> Rs. {invoice.TotalAmount}</p>
                    <p style='margin: 5px 0 0 0; color: #e74c3c;'><strong>Status:</strong> OVERDUE</p>
                </div>
                
                <p>Please contact us immediately to arrange payment.</p>
                
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='http://localhost:5173/customer/dashboard' 
                       style='background-color: #f39c12; color: #000; padding: 10px 20px; 
                              text-decoration: none; border-radius: 5px; font-weight: bold;'>
                        View Dashboard
                    </a>
                </div>
            </div>
            
            <div style='background-color: #1a1a2e; padding: 20px; text-align: center;'>
                <p style='color: #888; font-size: 11px; margin: 0;'>GearUp Vehicle Parts System</p>
            </div>
        </div>
        ";

            await _emailService.SendEmailAsync(
                user.Email!,
                $"{user.FirstName} {user.LastName}",
                subject,
                body);

            // Create notification for admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var admin = admins.FirstOrDefault();
            if (admin != null)
            {
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = admin.Id,
                    Type = NotificationType.CreditReminder,
                    Message = $"Overdue reminder sent to {user.FirstName} {user.LastName} for Invoice #{invoice.Id} — Rs. {invoice.TotalAmount}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "Overdue reminder sent to {Email} for Invoice #{InvoiceId}.",
                user.Email, invoice.Id);
        }
    }

}