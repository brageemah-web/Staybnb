using staybnb.Data;
using staybnb.Models;

namespace staybnb.Services;

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string? body, NotificationType type, string? linkUrl = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationContext _context;

    public NotificationService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task NotifyAsync(string userId, string title, string? body, NotificationType type, string? linkUrl = null){
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();
    }
}