using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BookingSite.Models;
using BookingSite.Models.ViewModels;
using BookingSite.Data;
using BookingSite.Services;

namespace BookingSite.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDayMappingService _dayMappingService;

        public BookingController(
            ApplicationDbContext context, 
            ILogger<BookingController> logger,
            UserManager<ApplicationUser> userManager,
            IDayMappingService dayMappingService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _dayMappingService = dayMappingService;
        }

        public async Task<IActionResult> Book(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Today;
            var now = DateTime.Now;
            
            // Get all rooms
            var rooms = await _context.Rooms.ToListAsync();
            
            // Get time slots for each room
            var roomTimeSlots = new Dictionary<int, List<TimeSlot>>();
            foreach (var room in rooms)
            {
                var mappedDay = _dayMappingService.MapDayOfWeek(selectedDate.DayOfWeek);
                var openDays = room.OpenDays.Split(',').Select(int.Parse).ToList();
                
                _logger.LogInformation($"Room {room.Name}: Selected Date: {selectedDate.DayOfWeek}, Mapped Day: {mappedDay}, OpenDays: {string.Join(",", openDays)}");
                
                if (openDays.Contains(mappedDay))
                {
                    var slots = await _context.TimeSlots
                        .Where(ts => ts.RoomId == room.Id)
                        .ToListAsync();

                    // Filter out past time slots
                    if (selectedDate.Date == now.Date)
                    {
                        slots = slots.Where(ts => ts.StartTime.ToTimeSpan() > now.TimeOfDay).ToList();
                    }
                    
                    roomTimeSlots[room.Id] = slots;
                }
                else
                {
                    // Room is not open on this day, set empty list
                    roomTimeSlots[room.Id] = new List<TimeSlot>();
                }
            }

            var viewModel = new BookingCalendarViewModel
            {
                Rooms = rooms,
                SelectedDate = selectedDate,
                RoomTimeSlots = roomTimeSlots
            };

            return View(viewModel);
        }

        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Check if the room is open on the selected day
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null)
            {
                return BadRequest("Room not found.");
            }

            var mappedDay = _dayMappingService.MapDayOfWeek(model.Date.DayOfWeek);
            var openDays = room.OpenDays.Split(',').Select(int.Parse).ToList();
            
            _logger.LogInformation($"Booking attempt - Room {room.Name}: Selected Date: {model.Date.DayOfWeek}, Mapped Day: {mappedDay}, OpenDays: {string.Join(",", openDays)}");
            
            if (!openDays.Contains(mappedDay))
            {
                return BadRequest("The room is not open on the selected day.");
            }

            // Check if the time slot is available
            var timeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(ts => ts.Id == model.TimeSlotId && ts.RoomId == model.RoomId);

            if (timeSlot == null || timeSlot.IsOccupied || !timeSlot.IsEnabled)
            {
                return BadRequest("The selected time slot is not available.");
            }

            // Create the booking
            var booking = new Booking
            {
                RoomId = model.RoomId,
                TimeSlotId = model.TimeSlotId,
                Date = model.Date,
                UserId = user.Id,
                BookedAt = DateTime.UtcNow
            };

            // Mark the time slot as occupied
            timeSlot.IsOccupied = true;
            timeSlot.BookedByUserId = user.Id;
            timeSlot.BookedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var booking = await _context.Bookings
                .Include(b => b.TimeSlot)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            // Only allow cancellation of future bookings
            if (booking.Date.Date < DateTime.Today)
            {
                return BadRequest("Cannot cancel past bookings.");
            }

            // Mark the time slot as available
            booking.TimeSlot.IsOccupied = false;
            booking.TimeSlot.BookedByUserId = null;
            booking.TimeSlot.BookedAt = null;

            // Remove the booking
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
} 