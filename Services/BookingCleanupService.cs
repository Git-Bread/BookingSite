using Microsoft.EntityFrameworkCore;
using BookingSite.Data;
using BookingSite.Models;

namespace BookingSite.Services
{
    public class BookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public BookingCleanupService(
            IServiceProvider serviceProvider,
            ILogger<BookingCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldBookings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up old bookings");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanupOldBookings()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;
            var oldBookings = await context.Bookings
                .Include(b => b.TimeSlot)
                .Where(b => b.Date < now.Date || (b.Date == now.Date && b.TimeSlot.EndTime < TimeOnly.FromTimeSpan(now.TimeOfDay)))
                .ToListAsync();

            foreach (var booking in oldBookings)
            {
                try
                {
                    // Mark the time slot as available
                    booking.TimeSlot.IsOccupied = false;
                    booking.TimeSlot.BookedByUserId = null;
                    booking.TimeSlot.BookedAt = null;

                    // Remove the booking
                    context.Bookings.Remove(booking);
                    _logger.LogInformation($"Automatically cancelled booking {booking.Id} for room {booking.RoomId} on {booking.Date}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error cancelling booking {booking.Id}");
                }
            }

            await context.SaveChangesAsync();
        }
    }
} 