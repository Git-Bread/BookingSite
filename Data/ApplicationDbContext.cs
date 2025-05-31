using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookingSite.Models;

namespace BookingSite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Room
            builder.Entity<Room>()
                .HasMany(r => r.TimeSlots)
                .WithOne(ts => ts.Room)
                .HasForeignKey(ts => ts.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TimeSlot
            builder.Entity<TimeSlot>()
                .HasOne(ts => ts.BookedByUser)
                .WithMany()
                .HasForeignKey(ts => ts.BookedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Booking
            builder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany()
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.TimeSlot)
                .WithMany()
                .HasForeignKey(b => b.TimeSlotId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraint to prevent double bookings
            builder.Entity<Booking>()
                .HasIndex(b => new { b.RoomId, b.TimeSlotId, b.Date })
                .IsUnique();
        }
    }
} 