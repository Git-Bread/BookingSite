using System;
using System.Collections.Generic;

namespace BookingSite.Models.Temp;

public partial class Booking
{
    public int Id { get; set; }

    public string Date { get; set; } = null!;

    public int RoomId { get; set; }

    public int TimeSlotId { get; set; }

    public string UserId { get; set; } = null!;

    public string BookedAt { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;

    public virtual TimeSlot TimeSlot { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
