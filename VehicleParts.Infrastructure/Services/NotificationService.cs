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
    private const int LowStockThreshold = 10;

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
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count == 0)
        {
            return;
        }

        var today = DateTime.UtcNow.Date;
        var message = $"Low stock alert: '{partName}' has only {currentStock} units remaining. Please restock soon.";

        foreach (var admin in admins)
        {
            var exists = await _context.Notifications.AnyAsync(n =>
                n.UserId == admin.Id &&
                n.Type == NotificationType.LowStock &&
                n.Message == message &&
                n.CreatedAt >= today);

            if (exists)
            {
                continue;
            }

            await _notificationRepository.CreateAsync(new Notification
            {
                UserId = admin.Id,
                Type = NotificationType.LowStock,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

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
            UserId = n.UserId,
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

    public async Task SendCreditRemindersAsync()
    {
        await ProcessOverdueCreditRemindersAsync(CancellationToken.None);
    }

    public async Task ProcessAutomatedNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var lowStockParts = await _context.Parts
            .Where(p => p.StockQty < LowStockThreshold)
            .OrderBy(p => p.StockQty)
            .ToListAsync(cancellationToken);

        foreach (var part in lowStockParts)
        {
            await CreateLowStockNotificationAsync(part.Name, part.StockQty);
        }

        await ProcessOverdueCreditRemindersAsync(cancellationToken);
    }

    private async Task ProcessOverdueCreditRemindersAsync(CancellationToken cancellationToken)
    {
        var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

        var overdueInvoices = await _context.SalesInvoices
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Where(s => (s.PaymentStatus == PaymentStatus.Credit || s.PaymentStatus == PaymentStatus.Overdue)
                     && s.Date <= oneMonthAgo)
            .ToListAsync(cancellationToken);

        foreach (var invoice in overdueInvoices)
        {
            var customer = invoice.Customer;
            var user = customer.User;
            var fullName = $"{user.FirstName} {user.LastName}";

            if (invoice.PaymentStatus == PaymentStatus.Credit)
            {
                invoice.PaymentStatus = PaymentStatus.Overdue;
            }

            var subject = "Payment Reminder - VehicleParts";
            var body = $@"
            <h2>Payment Reminder</h2>
            <p>Dear {fullName},</p>
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
                fullName,
                subject,
                body);

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                await _notificationRepository.CreateAsync(new Notification
                {
                    UserId = admin.Id,
                    Type = NotificationType.CreditReminder,
                    Message = $"Credit reminder sent to {fullName} for Invoice #{invoice.Id} - Rs. {invoice.TotalAmount}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _logger.LogInformation(
                "Credit reminder sent to {Email} for Invoice #{InvoiceId}.",
                user.Email, invoice.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
