namespace BookingSite.Models.ViewModels
{
    public class UpdateRoomDaysViewModel
    {
        public int RoomId { get; set; }
        public int[] OpenDays { get; set; } = Array.Empty<int>();
    }
} 