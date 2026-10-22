using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;

namespace staybnb.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly ApplicationContext _context;
    public NotificationBellViewComponent(ApplicationContext context){
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        var count = userId == null ? 0 : await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        return View(count);
    }
}