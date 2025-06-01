using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BookingSite.Models;
using BookingSite.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using BookingSite.Data;

namespace BookingSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.Rooms
                .Include(r => r.TimeSlots)
                .Select(r => new PublicRoomViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Location = r.Location,
                    OpenDays = r.OpenDays,
                    AvailableTimeSlots = r.TimeSlots
                        .Where(ts => ts.IsEnabled && !ts.IsOccupied)
                        .OrderBy(ts => ts.StartTime)
                        .Select(ts => new TimeSlotViewModel
                        {
                            Id = ts.Id,
                            StartTime = ts.StartTime,
                            EndTime = ts.EndTime,
                            IsEnabled = ts.IsEnabled,
                            IsAvailable = !ts.IsOccupied
                        })
                        .ToList()
                })
                .ToListAsync();

            return View(rooms);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 