using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class CheckInProcess
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string StepsJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
