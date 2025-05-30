using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSite.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required]
        public bool IsEnabled { get; set; } = false; // Disabled by default

        [Required]
        public bool IsOccupied { get; set; } = false;

        // Foreign key for Room
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // Optional foreign key for the user who booked the slot
        public string? BookedByUserId { get; set; }
        [ForeignKey("BookedByUserId")]
        public ApplicationUser? BookedByUser { get; set; }

        // Booking date (when this slot was booked)
        public DateTime? BookedAt { get; set; }
    }
} 