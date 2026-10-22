using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;

namespace staybnb.Services;

public record BookingQuote(int Nights, decimal Subtotal, decimal CleaningFee, decimal ServiceFee, decimal Total);

public interface IBookingService
{
    BookingQuote Calculate(HostProperty property, DateTime checkIn, DateTime checkOut);
    Task<bool> IsAvailableAsync(int propertyId, DateTime checkIn, DateTime checkOut);
}

public class BookingService : IBookingService{
    public const decimal ServiceFeeRate = 0.12m;   

    private readonly ApplicationContext _context;
    public BookingService(ApplicationContext context) { 
        _context = context;
    }
    public BookingQuote Calculate(HostProperty property, DateTime checkIn, DateTime checkOut){
        var nights = (checkOut.Date - checkIn.Date).Days;
        var subtotal = property.PricePerNight * nights;
        var serviceFee = Math.Round(subtotal * ServiceFeeRate, 2);
        var total = subtotal + property.CleaningFee + serviceFee;
        return new BookingQuote(nights, subtotal, property.CleaningFee, serviceFee, total);
    }

    public async Task<bool> IsAvailableAsync(int propertyId, DateTime checkIn, DateTime checkOut){
        var taken = await _context.Bookings.AnyAsync(b =>
            b.PropertyId == propertyId &&
            (b.Status == BookingStatus.Pending ||
             b.Status == BookingStatus.Approved ||
             b.Status == BookingStatus.CheckedIn) &&
            b.CheckInDate < checkOut.Date && b.CheckOutDate > checkIn.Date);
        return !taken;
    }
}