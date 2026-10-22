using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace staybnb.Models
{
    public class Message
    {
        public int Id { get; set; }

        [ForeignKey(nameof(SenderId))]
        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;

        [ForeignKey(nameof(ReceiverId))]
        public string ReceiverId { get; set; } = null!;
        public ApplicationUser Receiver { get; set; } = null!;

        [Required]
        public string Content { get; set; } = "";

     
        public DateTime Timestamp { get;  set; }
        public bool IsRead { get; set; }
    }
}
