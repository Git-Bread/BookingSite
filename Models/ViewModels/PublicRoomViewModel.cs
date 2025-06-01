namespace BookingSite.Models.ViewModels
{
    public class PublicRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string OpenDays { get; set; } = string.Empty;
        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public int Id { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAvailable { get; set; }
    }
} 