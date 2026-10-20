using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        public ActivityType ActivityType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
