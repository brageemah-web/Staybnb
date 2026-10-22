using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;
using staybnb.Services;
using staybnb.ViewModels;

namespace staybnb.Controllers;

[Authorize(Roles = "Guest")]
public class HostApplicationController : Controller
{
    private readonly ApplicationContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileService _files;
    private readonly IActivityLogService _log;
    private readonly INotificationService _notify;

    public HostApplicationController(ApplicationContext db,
        UserManager<ApplicationUser> userManager, IFileService files,
        IActivityLogService log, INotificationService notify)
    {
        _db = db; _userManager = userManager; _files = files;
        _log = log; _notify = notify;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = _userManager.GetUserId(User)!;
        var hasPending = await _db.HostApplications.AnyAsync(a =>
            a.ApplicationUserId == userId && a.Status == ApplicationStatus.Pending);

        if (hasPending)
        {
            TempData["Message"] = "You already have an application under review.";
            return RedirectToAction(nameof(Status));
        }

        var model = new HostApplicationViewModel();
        await LoadAmenities(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HostApplicationViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Server-side rule from the brief: no image, no application
        if (model.Images == null || model.Images.Count == 0)
            ModelState.AddModelError(nameof(model.Images), "At least one image is required.");
        else if (model.Images.Any(i => !FileService.IsAllowedImage(i)))
            ModelState.AddModelError(nameof(model.Images), "Only JPG, PNG or WEBP images up to 5 MB are allowed.");

        if (!ModelState.IsValid)
        {
            await LoadAmenities(model);
            return View(model);
        }

        var property = new HostProperty
        {
            Title = model.Title,
            Description = model.Description,
            Address = model.Address,
            City = model.City,
            PropertyType = model.PropertyType,
            PricePerNight = model.PricePerNight,
            CleaningFee = model.CleaningFee,
            MaxGuests = model.MaxGuests,
            Bedrooms = model.Bedrooms,
            Bathrooms = model.Bathrooms,
            HostId = user.Id,
            IsActive = false          // not listed until an admin approves
        };

        var amenities = await _db.Amenities
            .Where(a => model.SelectedAmenityIds.Contains(a.Id)).ToListAsync();
        foreach (var amenity in amenities) property.Amenities.Add(amenity);

        foreach (var file in model.Images)
        {
            var url = await _files.SaveAsync(file, "properties");
            property.Images.Add(new PropertyImage { ImageUrl = url });
        }

        // Property and application are saved together in one SaveChanges
        _db.HostApplications.Add(new HostApplication
        {
            ApplicationUserId = user.Id,
            Property = property,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await _log.LogAsync(user.Id, ActivityType.HostApplicationSubmitted,
            "Host application submitted", $"Property: {property.Title}");

        await _notify.NotifyAsync(user.Id, "Application submitted",
            "Your host application is under review.",
            NotificationType.HostApplicationSubmitted, Url.Action(nameof(Status)));

        // Tell every admin there is something to review
        var admins = (await _userManager.GetUsersInRoleAsync("Admin"))
            .Concat(await _userManager.GetUsersInRoleAsync("SuperAdmin"))
            .DistinctBy(u => u.Id);
        foreach (var admin in admins)
            await _notify.NotifyAsync(admin.Id, "New host application",
                $"{user.FirstName} {user.LastName} applied with \"{property.Title}\".",
                NotificationType.HostApplicationSubmitted);

        TempData["Message"] = "Application submitted. An admin will review it soon.";
        return RedirectToAction(nameof(Status));
    }

    public async Task<IActionResult> Status()
    {
        var userId = _userManager.GetUserId(User)!;
        var applications = await _db.HostApplications
            .Include(a => a.Property)
            .Where(a => a.ApplicationUserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
        return View(applications);
    }

    private async Task LoadAmenities(HostApplicationViewModel model) =>
        model.AvailableAmenities = await _db.Amenities.OrderBy(a => a.Name).ToListAsync();
}