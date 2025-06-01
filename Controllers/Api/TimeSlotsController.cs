using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingSite.Data;
using BookingSite.Models;

namespace BookingSite.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeSlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TimeSlots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeSlot>>> GetTimeSlots()
        {
            return await _context.TimeSlots.ToListAsync();
        }

        // GET: api/TimeSlots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlot>> GetTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);

            if (timeSlot == null)
            {
                return NotFound();
            }

            return timeSlot;
        }

        // POST: api/TimeSlots
        [HttpPost]
        public async Task<ActionResult<TimeSlot>> CreateTimeSlot(TimeSlot timeSlot)
        {
            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTimeSlot), new { id = timeSlot.Id }, timeSlot);
        }

        // PUT: api/TimeSlots/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeSlot(int id, TimeSlot timeSlot)
        {
            if (id != timeSlot.Id)
            {
                return BadRequest();
            }

            _context.Entry(timeSlot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeSlotExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/TimeSlots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null)
            {
                return NotFound();
            }

            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TimeSlotExists(int id)
        {
            return _context.TimeSlots.Any(e => e.Id == id);
        }
    }
} 