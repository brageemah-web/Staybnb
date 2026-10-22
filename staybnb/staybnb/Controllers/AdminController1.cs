using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;
using staybnb.Services;

namespace staybnb.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _log;
    private readonly INotificationService _notify;

    public AdminController(ApplicationContext context, UserManager<ApplicationUser> userManager, IActivityLogService log, INotificationService notify){
        _context = context; 
        _userManager = userManager; 
        _log = log; 
        _notify = notify;
    }

    public async Task<IActionResult> Dashboard(){
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.PendingApps = await _context.HostApplications.CountAsync(a => a.Status == ApplicationStatus.Pending);
        ViewBag.ActiveProperties = await _context.HostProperties.CountAsync(p => p.IsActive);
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();

        var recent = await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .ToListAsync();
        return View(recent);
    }

    public async Task<IActionResult> Applications(){
        var apps = await _context.HostApplications
            .Include(a => a.Property)
            .Include(a => a.ApplicationUser)
            .OrderBy(a => a.Status)          
            .ThenByDescending(a => a.AppliedAt)
            .ToListAsync();
        return View(apps);
    }

    public async Task<IActionResult> ActivityLogs(ActivityType? type, int page = 1){
        const int pageSize = 25;
        if (page < 1) { 
            page = 1; 
        }

        var query = _context.ActivityLogs.Include(a => a.User).AsQueryable();
        if (type.HasValue) { 
            query = query.Where(a => a.ActivityType == type.Value); 
        }

        var total = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        ViewBag.Type = type;
        return View(logs);
    }

    public async Task<IActionResult> Review(int id)
    {
        var app = await _context.HostApplications
            .Include(a => a.ApplicationUser)
            .Include(a => a.Property).ThenInclude(p => p.Images)
            .Include(a => a.Property).ThenInclude(p => p.Amenities)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) return NotFound();
        return View(app);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id){
        var app = await _context.HostApplications
            .Include(a => a.ApplicationUser)
            .Include(a => a.Property)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null){
            return NotFound();
        }

        if (app.Status != ApplicationStatus.Pending){
            TempData["Message"] = "This application has already been reviewed.";
            return RedirectToAction(nameof(Applications));
        }

        var admin = await _userManager.GetUserAsync(User);
        var applicant = app.ApplicationUser;

        
        if (!await _userManager.IsInRoleAsync(applicant, "Host")) { 
            var added = await _userManager.AddToRoleAsync(applicant, "Host");

            if (!added.Succeeded){
                TempData["Message"] = "Could not add the Host role: " + string.Join(", ", added.Errors.Select(e => e.Description));

                return RedirectToAction(nameof(Review), new { id });
            }
        }
        if (await _userManager.IsInRoleAsync(applicant, "Guest")){
            await _userManager.RemoveFromRoleAsync(applicant, "Guest");
        }

        await _userManager.UpdateSecurityStampAsync(applicant);

        app.Status = ApplicationStatus.Approved;
        app.ReviewedAt = DateTime.Now;
        app.Property.IsActive = true;
        await _context.SaveChangesAsync();

        await _log.LogAsync(admin!.Id, ActivityType.RoleChange, "Guest role replaced by Host", $"User: {applicant.Email}");
        await _log.LogAsync(admin.Id, ActivityType.HostApplicationReviewed,"Host application approved", $"Application #{app.Id}: {app.Property.Title}");

        await _notify.NotifyAsync(applicant.Id, "Host application approved", $"Congratulations! Your application for \"{app.Property.Title}\" has been approved.",
            NotificationType.HostApplicationApproved);

        TempData["Message"] = $"{applicant.Email} is now a Host.";

        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason){
        var app = await _context.HostApplications
            .Include(a => a.ApplicationUser)
            .Include(a => a.Property)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) { 
            return NotFound(); 
        }

        if (app.Status != ApplicationStatus.Pending){
            TempData["Message"] = "This application has already been reviewed.";
            return RedirectToAction(nameof(Applications));
        }

        var admin = await _userManager.GetUserAsync(User);

        app.Status = ApplicationStatus.Rejected;
        app.ReviewedAt = DateTime.Now;
        app.RejectionReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await _context.SaveChangesAsync();

        await _log.LogAsync(admin!.Id, ActivityType.HostApplicationReviewed,"Host application rejected", $"Application #{app.Id}: {app.Property.Title}");

        await _notify.NotifyAsync(app.ApplicationUser.Id, "Host application rejected", app.RejectionReason ?? "Your application was not approved.",
            NotificationType.HostApplicationRejected);

        TempData["Message"] = "Application rejected.";

        return RedirectToAction(nameof(Applications));
    }
}