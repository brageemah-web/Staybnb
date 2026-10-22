using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;

namespace staybnb.Controllers;

[Authorize]   
public class NotificationsController : Controller
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(ApplicationContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context; _userManager = userManager;
    }

    public async Task<IActionResult> Index(bool unreadOnly = false){
        var userId = _userManager.GetUserId(User)!;
        var mine = _context.Notifications.Where(n => n.UserId == userId);
        ViewBag.UnreadCount = await mine.CountAsync(n => !n.IsRead);
        ViewBag.TotalCount = await mine.CountAsync();
        ViewBag.UnreadOnly = unreadOnly;

        if (unreadOnly) { 
            mine = mine.Where(n => !n.IsRead); 
        }

        var list = await mine.OrderByDescending(n => n.CreatedAt).Take(100).ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id){
        var userId = _userManager.GetUserId(User)!;
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n == null){
            return NotFound();
        }
        n.IsRead = true;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(){
        var userId = _userManager.GetUserId(User)!;
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n == null) return NotFound();

        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(int id) {
        var userId = _userManager.GetUserId(User)!;
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (n == null) {
            return NotFound();
        }
        n.IsRead = true;
        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(n.LinkUrl) && Url.IsLocalUrl(n.LinkUrl)) { 
            return Redirect(n.LinkUrl);
        }
        return RedirectToAction(nameof(Index));
    }
}