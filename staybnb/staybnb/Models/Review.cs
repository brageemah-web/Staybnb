using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class Review
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Property))]
        public int PropertId { get; set; }
        public HostProperty Property { get; set; } = null!;

        [ForeignKey(nameof(Reviewer))]
        public string ReviewerId { get; set; } = null!;
        public ApplicationUser Reviewer { get; set; } = null!;

        [Range(1,5)]
        public int Rating { get; set; }
        
        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
