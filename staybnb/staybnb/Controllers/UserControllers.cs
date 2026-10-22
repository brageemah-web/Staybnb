using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using staybnb.Models;
using staybnb.Services;

namespace staybnb.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _log;
    private readonly INotificationService _notify;

    public UsersController(UserManager<ApplicationUser> userManager,IActivityLogService log, INotificationService notify){
        _userManager = userManager; 
        _log = log; 
        _notify = notify;
    }

    public async Task<IActionResult> Index(string? search){
        var guests = (await _userManager.GetUsersInRoleAsync("Guest")).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search)){
            guests = guests.Where(u =>
                (u.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                $"{u.FirstName} {u.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        ViewBag.Search = search;
        return View(guests.OrderBy(u => u.Email).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(string id){
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) { 
            return NotFound(); 
        }

        if (!await _userManager.IsInRoleAsync(user, "Guest")){
            TempData["Message"] = "Only Guests can be promoted to Admin.";
            return RedirectToAction(nameof(Index));
        }

        var added = await _userManager.AddToRoleAsync(user, "Admin");
        if (!added.Succeeded){
            TempData["Message"] = "Could not promote user: " +
                string.Join(", ", added.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _userManager.RemoveFromRoleAsync(user, "Guest");
        await _userManager.UpdateSecurityStampAsync(user);   

        var superAdmin = await _userManager.GetUserAsync(User);
        await _log.LogAsync(superAdmin!.Id, ActivityType.RoleChange, "Guest promoted to Admin", $"User: {user.Email}");

        await _notify.NotifyAsync(user.Id, "You are now an Admin", "A SuperAdmin has promoted your account to Admin.",
            NotificationType.Success);

        TempData["Message"] = $"{user.Email} is now an Admin.";
        return RedirectToAction(nameof(Index));
    }
}