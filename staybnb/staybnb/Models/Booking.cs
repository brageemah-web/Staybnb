using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace staybnb.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        [ForeignKey(nameof(Guest))]
        public string GuestId { get; set; } = null!;
        public ApplicationUser Guest { get; set; } = null!;

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        [Precision(18,2)]
        public decimal TotalPrice { get; set; }

        [Precision(18, 2)]
        public decimal CleaningFee { get; set; }

        [Precision(18, 2)]
        public decimal ServiceFee { get; set; }
        public BookingStatus Status{ get; set; } = BookingStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public int Nights => (CheckOutDate.Date - CheckInDate.Date).Days;

        public Payment? Payment { get; set; }
        public GuestCheckIn? GuestCheckIn { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
