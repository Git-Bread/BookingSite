using System.ComponentModel.DataAnnotations;

namespace BookingSite.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        // Represents days the room is open, stored as a comma-separated string of day numbers (1-7)
        [Required]
        public string OpenDays { get; set; } = "1,2,3,4,5"; // Default Mon-Fri

        // Navigation property for time slots
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
} 