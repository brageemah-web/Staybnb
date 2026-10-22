using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using staybnb.Data;
using staybnb.Models;
using staybnb.Services;

namespace staybnb.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationContext _context;
    public PropertiesController(ApplicationContext context){
        _context = context;
    }
    public async Task<IActionResult> Index(string? city, PropertyType? type) {
        var query = _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(city)) {
            query = query.Where(p => p.City.Contains(city));
        }
        if (type.HasValue) { 
            query = query.Where(p => p.PropertyType == type.Value);
        }
        ViewBag.City = city;
        ViewBag.Type = type;
        return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id){
        var property = await _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Amenities)
            .Include(p => p.Host)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (property == null) { 
            return NotFound(); 
        }

        ViewBag.ServiceFeeRate = BookingService.ServiceFeeRate;
        return View(property);
    }
}