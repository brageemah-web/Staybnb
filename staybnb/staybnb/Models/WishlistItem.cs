using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class WishListItem
    {
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; } 
        public HostProperty Property { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
