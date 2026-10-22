using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;
using staybnb.Services;
using staybnb.ViewModels;

namespace staybnb.Controllers;

[Authorize]   
public class MessagesController : Controller{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notify;

    public MessagesController(ApplicationContext context, UserManager<ApplicationUser> userManager,INotificationService notify) { 
         _context = context; 
        _userManager = userManager; 
        _notify = notify;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        var threads = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>{
                var last = g.First();   
                return new ConversationSummary
                {
                    Other = last.SenderId == userId ? last.Receiver : last.Sender,
                    Last = last,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .ToList();

        return View(threads);
    }

    public async Task<IActionResult> Conversation(string id)
    {
        var userId = _userManager.GetUserId(User)!;
        if (id == userId){
            return RedirectToAction(nameof(Index));
        }
        var other = await _userManager.FindByIdAsync(id);
        if (other == null) { 
            return NotFound(); 
        }

        await _context.Messages
            .Where(m => m.SenderId == id && m.ReceiverId == userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

        var thread = await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == id) ||
                        (m.SenderId == id && m.ReceiverId == userId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        ViewBag.Other = other;
        ViewBag.UserId = userId;
        return View(thread);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, string? content) {
        var sender = await _userManager.GetUserAsync(User);
        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (sender == null || receiver == null || receiver.Id == sender.Id) { 
            return NotFound(); 
        }

        content = content?.Trim() ?? "";
        if (content.Length == 0 || content.Length > 2000){
            TempData["Error"] = "Message must be between 1 and 2000 characters.";
            return RedirectToAction(nameof(Conversation), new { id = receiverId });
        }

        _ = _context.Messages.Add(new Message{
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            Content = content,
            Timestamp = DateTime.Now,
            IsRead = false
        });
        await _context.SaveChangesAsync();

        await _notify.NotifyAsync(receiver.Id, "New message",$"{sender.FirstName} {sender.LastName} sent you a message.",
            NotificationType.NewMessage, Url.Action(nameof(Conversation), "Messages", new { id = sender.Id }));

        return RedirectToAction(nameof(Conversation), new { id = receiverId });
    }
}