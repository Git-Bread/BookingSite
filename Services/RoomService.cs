using BookingSite.Models;
using BookingSite.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingSite.Services
{
    public class RoomService
    {
        private readonly ApplicationDbContext _context;

        public RoomService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Room>> GetAllRoomsWithTimeSlotsAsync()
        {
            return await _context.Rooms
                .Include(r => r.TimeSlots)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            // Create default time slots for the room (9 AM to 5 PM, 1-hour slots)
            var timeSlots = new List<TimeSlot>();
            for (int hour = 9; hour < 17; hour++)
            {
                timeSlots.Add(new TimeSlot
                {
                    StartTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(hour)),
                    EndTime = TimeOnly.FromTimeSpan(TimeSpan.FromHours(hour + 1)),
                    IsEnabled = false, // Set to disabled by default
                    Room = room
                });
            }

            room.TimeSlots = timeSlots;
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<bool> UpdateTimeSlotStatusAsync(int timeSlotId, bool isEnabled)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(timeSlotId);
            if (timeSlot == null)
            {
                return false;
            }

            try
            {
                timeSlot.IsEnabled = isEnabled;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoomAsync(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.TimeSlots)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                return false;

            _context.TimeSlots.RemoveRange(room.TimeSlots);
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime date, int timeSlotId)
        {
            var room = await _context.Rooms
                .Include(r => r.TimeSlots)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                return false;

            // Check if the room is open on this day
            var dayOfWeek = ((int)date.DayOfWeek + 1) % 7 + 1; // Convert to 1-7 format
            if (!room.OpenDays.Split(',').Select(int.Parse).Contains(dayOfWeek))
                return false;

            // Check if the time slot exists and is enabled
            var timeSlot = room.TimeSlots.FirstOrDefault(ts => ts.Id == timeSlotId);
            if (timeSlot == null || !timeSlot.IsEnabled)
                return false;

            // Check if there's no existing booking for this room, date, and time slot
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId && b.Date.Date == date.Date && b.TimeSlotId == timeSlotId);

            return !existingBooking;
        }

        public async Task<bool> CreateBookingAsync(int roomId, int timeSlotId, string userId, DateTime date)
        {
            // Check if the time slot is enabled and the room is open on that day
            var room = await _context.Rooms
                .Include(r => r.TimeSlots)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null) return false;

            var timeSlot = room.TimeSlots.FirstOrDefault(t => t.Id == timeSlotId);
            if (timeSlot == null || !timeSlot.IsEnabled) return false;

            // Check if the room is open on this day
            var dayOfWeek = ((int)date.DayOfWeek + 6) % 7 + 1; // Convert to 1-7 (Monday-Sunday)
            if (!room.OpenDays.Split(',').Select(int.Parse).Contains(dayOfWeek))
                return false;

            // Check for existing booking
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId && 
                              b.TimeSlotId == timeSlotId && 
                              b.Date.Date == date.Date);

            if (existingBooking) return false;

            // Create the booking
            var booking = new Booking
            {
                RoomId = roomId,
                TimeSlotId = timeSlotId,
                UserId = userId,
                Date = date.Date
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Booking>> GetBookingsForDateAsync(DateTime date)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Where(b => b.Date.Date == date.Date)
                .OrderBy(b => b.Room.Name)
                .ThenBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.TimeSlot)
                .Where(b => b.UserId == userId && b.Date >= DateTime.Today)
                .OrderBy(b => b.Date)
                .ThenBy(b => b.TimeSlot.StartTime)
                .ToListAsync();
        }
    }
} 