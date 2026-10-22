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
public class GuestController : Controller
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingService _bookings;
    private readonly IActivityLogService _log;
    private readonly INotificationService _notify;
    private readonly IFileService _files;                    

    public GuestController(ApplicationContext context, UserManager<ApplicationUser> userManager,
        IBookingService bookings, IActivityLogService log, INotificationService notify,
        IFileService files)
    {
        _context = context; _userManager = userManager; _bookings = bookings;
        _log = log; _notify = notify; _files = files;
    }

    public async Task<IActionResult> MyBookings(){
        var userId = _userManager.GetUserId(User)!;
        var list = await _context.Bookings
            .Include(b => b.Property).ThenInclude(p => p.Images)
            .Include(b => b.Payment)
            .Where(b => b.GuestId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int propertyId, DateTime checkIn, DateTime checkOut, int guests)
    {
        var user = await _userManager.GetUserAsync(User);
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
        if (user == null || property == null){
            return NotFound();
        }

        string? error = null;
        if (checkIn.Date < DateTime.Today)
        {
            error = "Check-in cannot be in the past.";
        }
        else if (checkOut.Date <= checkIn.Date)
        {
            error = "Check-out must be after check-in.";
        }
        else if (guests < 1 || guests > property.MaxGuests)
        {
            error = $"This property sleeps 1 to {property.MaxGuests} guests.";
        }
        else if (!await _bookings.IsAvailableAsync(propertyId, checkIn, checkOut)){
            error = "Those dates are not available.";
        }

        if (error != null){
            TempData["Error"] = error;
            return RedirectToAction("Details", "Properties", new { id = propertyId });
        }

        
        var quote = _bookings.Calculate(property, checkIn, checkOut);

        var booking = new Booking{
            PropertyId = property.Id,
            GuestId = user.Id,
            CheckInDate = checkIn.Date,
            CheckOutDate = checkOut.Date,
            NumberOfGuests = guests,
            CleaningFee = quote.CleaningFee,
            ServiceFee = quote.ServiceFee,
            TotalPrice = quote.Total,
            Status = BookingStatus.Pending,
            Payment = new Payment { Amount = quote.Total, Status = PaymentStatus.Pending }
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        await _log.LogAsync(user.Id, ActivityType.BookingCreated, "Booking requested",$"Booking #{booking.Id} for \"{property.Title}\"");

        await _notify.NotifyAsync(property.HostId, "New booking request", $"{user.FirstName} {user.LastName} requested \"{property.Title}\" " +
            $"({checkIn:dd MMM} to {checkOut:dd MMM}).", NotificationType.BookingRequested);

        TempData["Message"] = "Booking requested. The host will review it soon.";
        return RedirectToAction(nameof(MyBookings));
    }

   
    private async Task<Booking?> GetOwnBookingAsync(int id){
        var userId = _userManager.GetUserId(User)!;
        return await _context.Bookings
            .Include(b => b.Property).ThenInclude(p => p.CheckInProcess)
            .Include(b => b.GuestCheckIn).ThenInclude(c => c!.Documents)
            .FirstOrDefaultAsync(b => b.Id == id && b.GuestId == userId);
    }

    public async Task<IActionResult> CheckIn(int id){
        var booking = await GetOwnBookingAsync(id);
        if (booking == null) { 
            return NotFound(); 
        }

        if (booking.Status != BookingStatus.Approved && booking.Status != BookingStatus.CheckedIn){
            TempData["Message"] = "Check-in is only available for approved bookings.";
            return RedirectToAction(nameof(MyBookings));
        }

        ViewBag.Config = booking.Property.CheckInProcess == null ? null : CheckInConfig.Parse(booking.Property.CheckInProcess.StepsJson);
        return View(booking);
    }

   
    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartCheckIn(int id)
    {
        var booking = await GetOwnBookingAsync(id);
        if (booking == null) return NotFound();

        if (booking.Status == BookingStatus.Approved &&
            booking.Property.CheckInProcess != null &&
            booking.GuestCheckIn == null)
        {
            booking.GuestCheckIn = new GuestCheckIn
            {
                CheckInProcessId = booking.Property.CheckInProcess.Id,
                Status = CheckInStatus.InProgress
            };
            await _context.SaveChangesAsync();

            await _log.LogAsync(booking.GuestId, ActivityType.CheckInUpdated,
                "Check-in started", $"Booking #{booking.Id}");
        }
        return RedirectToAction(nameof(CheckIn), new { id });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument(int id, string documentType, IFormFile? file)
    {
        var booking = await GetOwnBookingAsync(id);
        if (booking?.GuestCheckIn == null) {
            return NotFound();
        }

        var checkIn = booking.GuestCheckIn;
        var config = CheckInConfig.Parse(booking.Property.CheckInProcess?.StepsJson);

        string? error = null;
        if (checkIn.Status != CheckInStatus.InProgress && checkIn.Status != CheckInStatus.Rejected)
        {
            error = "Documents can't be changed at this stage.";
        }
        else if (file == null || !FileService.IsAllowedDocument(file))
        {
            error = "Upload a JPG, PNG or PDF of up to 5 MB.";
        }
        else if (!config.DocumentTypes.Contains(documentType)) { 
            error = "Choose one of the accepted document types."; 
        }

        if (error != null){
            TempData["Error"] = error;
            return RedirectToAction(nameof(CheckIn), new { id });
        }

        var stored = await _files.SaveDocumentAsync(file!);
        checkIn.Documents.Add(new GuestDocument{
            DocumentType = documentType,
            FileName = stored,
            Status = DocumentStatus.Pending
        });
        await _context.SaveChangesAsync();

        await _log.LogAsync(booking.GuestId, ActivityType.DocumentUploaded,"Document uploaded", $"{documentType} for booking #{booking.Id}");

        return RedirectToAction(nameof(CheckIn), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitCheckIn(int id){
        var booking = await GetOwnBookingAsync(id);
        if (booking?.GuestCheckIn == null) return NotFound();

        var checkIn = booking.GuestCheckIn;
        var canSubmit = (checkIn.Status == CheckInStatus.InProgress || checkIn.Status == CheckInStatus.Rejected)
                        && checkIn.Documents.Any(d => d.Status == DocumentStatus.Pending);
        if (!canSubmit){
            TempData["Error"] = "Upload a document before submitting.";
            return RedirectToAction(nameof(CheckIn), new { id });
        }

        checkIn.Status = CheckInStatus.Submitted;
        await _context.SaveChangesAsync();

        var guest = await _userManager.GetUserAsync(User);
        await _log.LogAsync(booking.GuestId, ActivityType.CheckInUpdated, "Check-in submitted", $"Booking #{booking.Id}");
        await _notify.NotifyAsync(booking.Property.HostId, "Check-in submitted",
            $"{guest!.FirstName} {guest.LastName} submitted check-in documents for \"{booking.Property.Title}\".",
            NotificationType.CheckInSubmitted);

        TempData["Message"] = "Check-in submitted. The host will verify your document.";
        return RedirectToAction(nameof(CheckIn), new { id });
    }
}