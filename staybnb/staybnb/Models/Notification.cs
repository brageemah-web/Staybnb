using System.ComponentModel.DataAnnotations;

namespace staybnb.Models;

public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = "";

    public string? Body { get; set; }
    public string? LinkUrl { get; set; }

    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}