using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class GuestDocument
    {
        public int Id { get; set; }

        [ForeignKey(nameof(GuestCheckIn))]
        public int GuestCheckInId { get; set; }
        public GuestCheckIn GuestCheckIn { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string DocumentType { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string FileName { get; set; } = "";

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    }
}
