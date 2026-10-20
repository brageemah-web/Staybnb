using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = "";
    }
}
