using staybnb.Data;
using staybnb.Models;

namespace staybnb.Services;

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string? body, NotificationType type, string? linkUrl = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationContext _db;

    public NotificationService(ApplicationContext db)
    {
        _db = db;
    }

    public async Task NotifyAsync(string userId, string title, string? body, NotificationType type, string? linkUrl = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }
}