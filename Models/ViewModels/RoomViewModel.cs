using System.ComponentModel.DataAnnotations;

namespace BookingSite.Models.ViewModels
{
    public class RoomViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // Array of weekday numbers (1-7) that the room is open
        [Required]
        public int[] OpenDays { get; set; } = new[] { 1, 2, 3, 4, 5 }; // Default Mon-Fri
    }

    public class TimeSlotManagementViewModel
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }
} 