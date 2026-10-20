using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;

namespace staybnb.Models
{
    public class HostApplcation
    {
        public int Id { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        [ForeignKey(nameof(Property))]
        public int PropertyId { get; set; }
        public HostProperty Property { get; set; } = null!;
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }
}
