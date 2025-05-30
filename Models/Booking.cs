using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingSite.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Foreign key for Room
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // Foreign key for TimeSlot
        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        // Foreign key for User
        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    }
} 