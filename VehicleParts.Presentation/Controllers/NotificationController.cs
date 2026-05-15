using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleParts.Application.Interfaces.IServices;

namespace VehicleParts.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _notificationService.GetAdminNotificationsAsync();
        return Ok(notifications);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new { message = "Notification marked as read." });
    }

    [HttpPost("send-credit-reminders")]
    public async Task<IActionResult> SendCreditReminders()
    {
        await _notificationService.SendCreditRemindersAsync();
        return Ok(new { message = "Credit reminders sent to all overdue customers." });
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessNotifications(CancellationToken cancellationToken)
    {
        await _notificationService.ProcessAutomatedNotificationsAsync(cancellationToken);
        return Ok(new { message = "Automated notifications processed successfully." });
    }
}
