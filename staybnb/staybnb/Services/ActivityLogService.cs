using staybnb.Data;
using staybnb.Models;


namespace staybnb.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(string userId, ActivityType type, string action, string? details = null);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationContext _context;
        public ActivityLogService(ApplicationContext context) { 
            _context = context; 
        }

        public async Task LogAsync(string userId, ActivityType type, string action, string? details = null)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                ActivityType = type,
                Action = action,
                Details = details,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}
