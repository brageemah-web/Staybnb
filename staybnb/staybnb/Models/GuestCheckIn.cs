using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class GuestCheckIn
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Booking))]
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = null!;


        [ForeignKey(nameof(CheckInProcess))]
        public int CheckInProcessId { get; set; }
        public CheckInProcess CheckInProcess { get; set; } = null!;


        public CheckInStatus Status { get; set; } = CheckInStatus.NotStarted;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<GuestDocument> Documents { get; set; } = new List<GuestDocument>();
    }
}
