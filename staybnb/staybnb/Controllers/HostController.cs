using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;
using staybnb.Services;
using staybnb.ViewModels;
using System.Text.Json;

namespace staybnb.Controllers;

[Authorize(Roles = "Host")]
public class HostController : Controller
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _log;
    private readonly INotificationService _notify;
    private readonly IFileService _files;                       

    public HostController(ApplicationContext context, UserManager<ApplicationUser> userManager, IActivityLogService log, INotificationService notify, IFileService files){
        _context = context;  
        _userManager = userManager; 
        _log = log; 
        _notify = notify; 
        _files = files;
    }

    public async Task<IActionResult > Dashboard(){
        var userId = _userManager.GetUserId(User)!;
        var user = await _userManager.GetUserAsync(User);

        var properties = _context.HostProperties.Where(p => p.HostId == userId);
        var bookings = _context.Bookings.Where(b => b.Property.HostId == userId);

        ViewBag.Name = user!.FirstName;
        ViewBag.TotalProperties = await properties.CountAsync();
        ViewBag.ActiveProperties = await properties.CountAsync(p => p.IsActive);
        ViewBag.TotalBookings = await bookings.CountAsync();
        ViewBag.PendingBookings = await bookings.CountAsync(b => b.Status == BookingStatus.Pending);
        ViewBag.Earnings = await bookings
            .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn)
            .SumAsync(b => (decimal?)b.TotalPrice) ?? 0m;

        var ratings = _context.Reviews.Where(r => r.Property.HostId == userId);
        ViewBag.AverageRating = await ratings.AnyAsync()
            ? await ratings.AverageAsync(r => (double)r.Rating) : 0.0;

        return View();
    }

    public async Task<IActionResult> MyProperties(){
        var userId = _userManager.GetUserId(User)!;

        var list = await _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Bookings)
            .Where(p => p.HostId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == userId);
        if (property == null) { 
            return NotFound(); 
        }

        property.IsActive = !property.IsActive;
        await _context.SaveChangesAsync();

        await _log.LogAsync(userId, ActivityType.PropertyStatusToggled, property.IsActive ? "Property activated" : "Property deactivated",
            $"Property #{property.Id}: {property.Title}");

        TempData["Message"] = property.IsActive ? $"\"{property.Title}\" is now visible to guests."
            : $"\"{property.Title}\" is hidden from guests.";
        return RedirectToAction(nameof(MyProperties));
    }

    public async Task<IActionResult> BookingRequests(){
        var userId = _userManager.GetUserId(User)!;

        var bookings = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .Include(b => b.Payment)
            .Where(b => b.Property.HostId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBooking(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == userId);
        if (booking == null) { 
            return NotFound();
        }

        if (booking.Status != BookingStatus.Pending){
            TempData["Message"] = "This booking has already been handled.";
            return RedirectToAction(nameof(BookingRequests));
        }

        booking.Status = BookingStatus.Approved;
        if (booking.Payment != null){
            booking.Payment.Status = PaymentStatus.Paid;
        }
        await _context.SaveChangesAsync();

        await _log.LogAsync(userId, ActivityType.BookingStatusUpdated, "Booking approved", $"Booking #{booking.Id}: Pending -> Approved");

        await _notify.NotifyAsync(booking.GuestId, "Booking approved", $"Your booking for \"{booking.Property.Title}\" ({booking.CheckInDate:dd MMM} to " +
            $"{booking.CheckOutDate:dd MMM}) was approved. You can now complete check-in.", NotificationType.BookingApproved);

        TempData["Message"] = "Booking approved.";
        return RedirectToAction(nameof(BookingRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectBooking(int id){
        var userId = _userManager.GetUserId(User)!;

        var booking = await _context.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == userId);
        if (booking == null){
            return NotFound();
        }

        if (booking.Status != BookingStatus.Pending){
            TempData["Message"] = "This booking has already been handled.";
            return RedirectToAction(nameof(BookingRequests));
        }

        booking.Status = BookingStatus.Rejected;
        await _context.SaveChangesAsync();

        await _log.LogAsync(userId, ActivityType.BookingStatusUpdated, "Booking rejected", $"Booking #{booking.Id}: Pending -> Rejected");

        await _notify.NotifyAsync(booking.GuestId, "Booking rejected", $"Your booking for \"{booking.Property.Title}\" was not accepted by the host.",
            NotificationType.BookingRejected);

        TempData["Message"] = "Booking rejected.";
        return RedirectToAction(nameof(BookingRequests));
    }

    [HttpGet]
    public async Task<IActionResult> CheckInSetup(int propertyId)
    {
        var userId = _userManager.GetUserId(User)!;
        var property = await _context.HostProperties.Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == userId);
        if (property == null){
            return NotFound();
        }
        var model = new CheckInSetupViewModel{
            PropertyId = property.Id,
            Title = "Guest check-in",
            DocumentTypes = new List<string> { "ID", "Passport" }
        };

        if (property.CheckInProcess != null){
            var config = CheckInConfig.Parse(property.CheckInProcess.StepsJson);
            model.Title = property.CheckInProcess.Title;
            model.Rules = config.Rules;
            model.DocumentTypes = config.DocumentTypes;
        }

        ViewBag.PropertyTitle = property.Title;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInSetup(CheckInSetupViewModel model) {
        var userId = _userManager.GetUserId(User)!;
        var property = await _context.HostProperties.Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == model.PropertyId && p.HostId == userId);
        if (property == null) {
            return NotFound();
        }

        if (model.DocumentTypes.Count == 0) {
            ModelState.AddModelError(nameof(model.DocumentTypes), "Select at least one accepted document type.");
        }
        if (!ModelState.IsValid) {
            ViewBag.PropertyTitle = property.Title;
            return View(model);
        }

        var json = JsonSerializer.Serialize(new CheckInConfig {
            Rules = model.Rules,
            DocumentTypes = model.DocumentTypes
        });

        if (property.CheckInProcess == null) { 
            property.CheckInProcess = new CheckInProcess { Title = model.Title, StepsJson = json }; 
        }
        else{
            property.CheckInProcess.Title = model.Title;
            property.CheckInProcess.StepsJson = json;
        }
        await _context.SaveChangesAsync();

        await _log.LogAsync(userId, ActivityType.PropertyUpdated, "Check-in process configured", $"Property #{property.Id}: {property.Title}");

        TempData["Message"] = "Check-in process saved.";
        return RedirectToAction(nameof(MyProperties));
    }

    public async Task<IActionResult> CheckIns()
    {
        var userId = _userManager.GetUserId(User)!;
        var list = await _context.GuestCheckIns
            .Include(c => c.Booking).ThenInclude(b => b.Property)
            .Include(c => c.Booking).ThenInclude(b => b.Guest)
            .Include(c => c.Documents)
            .Where(c => c.Booking.Property.HostId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> CheckInReview(int id){
        var userId = _userManager.GetUserId(User)!;
        var checkIn = await _context.GuestCheckIns
            .Include(c => c.Booking).ThenInclude(b => b.Property)
            .Include(c => c.Booking).ThenInclude(b => b.Guest)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id && c.Booking.Property.HostId == userId);
        if (checkIn == null) return NotFound();
        return View(checkIn);
    }

    public async Task<IActionResult> ViewDocument(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var doc = await _context.GuestDocuments
            .FirstOrDefaultAsync(d => d.Id == id && d.GuestCheckIn.Booking.Property.HostId == userId);
        if (doc == null) {
            return NotFound();
        }

        var path = _files.GetDocumentPath(doc.FileName);
        if (!System.IO.File.Exists(path)) {
            return NotFound();
        }

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch{
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            _ => "image/jpeg"
        };
        return PhysicalFile(path, contentType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewDocument(int id, bool approve){
        var userId = _userManager.GetUserId(User)!;

        var doc = await _context.GuestDocuments
            .Include(d => d.GuestCheckIn).ThenInclude(c => c.Booking).ThenInclude(b => b.Property)
            .Include(d => d.GuestCheckIn).ThenInclude(c => c.Documents)
            .FirstOrDefaultAsync(d => d.Id == id &&
                                      d.GuestCheckIn.Booking.Property.HostId == userId);
        if (doc == null) {
            return NotFound();
        }

        var checkIn = doc.GuestCheckIn;
        var booking = checkIn.Booking;

        if (doc.Status != DocumentStatus.Pending){
            TempData["Message"] = "This document has already been reviewed.";
            return RedirectToAction(nameof(CheckInReview), new { id = checkIn.Id });
        }

        doc.Status = approve ? DocumentStatus.Approved : DocumentStatus.Rejected;
        var verified = false;

        if (!approve){
            checkIn.Status = CheckInStatus.Rejected;
        }
        else if (!checkIn.Documents.Any(d => d.Status == DocumentStatus.Pending))
        {
            checkIn.Status = CheckInStatus.Verified;
            booking.Status = BookingStatus.CheckedIn;    
            verified = true;
        }
        await _context.SaveChangesAsync();

        await _log.LogAsync(userId, ActivityType.DocumentReviewed, approve ? "Document approved" : "Document rejected",
            $"Document #{doc.Id} for booking #{booking.Id}");

        if (verified){
            await _log.LogAsync(userId, ActivityType.BookingStatusUpdated, "Guest checked in", $"Booking #{booking.Id}: Approved -> CheckedIn");
            await _notify.NotifyAsync(booking.GuestId, "Check-in verified", $"Your check-in for \"{booking.Property.Title}\" is verified. Enjoy your stay!",
                NotificationType.CheckInVerified);
        }
        else if (!approve){
            await _notify.NotifyAsync(booking.GuestId, "Document rejected", $"Your {doc.DocumentType} for \"{booking.Property.Title}\" was rejected. Please upload a clearer copy.",
                NotificationType.Warning);
        }

        TempData["Message"] = approve ? "Document approved." : "Document rejected.";
        return RedirectToAction(nameof(CheckInReview), new { id = checkIn.Id });
    }
}