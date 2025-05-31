using System;
using System.Collections.Generic;

namespace BookingSite.Models.Temp;

public partial class TimeSlot
{
    public int Id { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int IsEnabled { get; set; }

    public int IsOccupied { get; set; }

    public int RoomId { get; set; }

    public string? BookedByUserId { get; set; }

    public string? BookedAt { get; set; }

    public virtual AspNetUser? BookedByUser { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Room Room { get; set; } = null!;
}
