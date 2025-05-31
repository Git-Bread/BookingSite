using BookingSite.Models;

namespace BookingSite.Models.ViewModels
{
    public class BookingCalendarViewModel
    {
        public List<Room> Rooms { get; set; } = new();
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public Dictionary<int, List<TimeSlot>> RoomTimeSlots { get; set; } = new();
    }

    public class CreateBookingViewModel
    {
        public int RoomId { get; set; }
        public int TimeSlotId { get; set; }
        public DateTime Date { get; set; }
    }
} 