using staybnb.Models;

namespace staybnb.ViewModels;

public class ConversationSummary
{
    public ApplicationUser Other { get; set; } = null!;
    public Message Last { get; set; } = null!;
    public int UnreadCount { get; set; }
}