using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class HostProperty
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Precision(18, 2)]
        public decimal PricePerNight { get; set; }

        [ForeignKey(nameof(Host))]
        public string HostId { get; set; } = null!;

        public ApplicationUser Host { get; set; } = null!;

        [Required]
        public string Address { get; set; } = "";

        [Required]
        public string City { get; set; } = "";

        public PropertyType PropertyType { get; set; }

        [Precision(18, 2)]
        public decimal CleaningFee { get; set; }

        public int MaxGuests { get; set; } = 1;

        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
        public ICollection<PropertyImage> Images {get; set; } = new List<PropertyImage>();
        public CheckInProcess? CheckInProcess { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    } 
}
