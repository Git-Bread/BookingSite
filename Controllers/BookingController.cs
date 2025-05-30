using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingSite.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        public IActionResult Book()
        {
            return View();
        }

        public IActionResult MyBookings()
        {
            return View();
        }
    }
} 