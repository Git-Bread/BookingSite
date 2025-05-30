using Microsoft.AspNetCore.Mvc;

namespace BookingSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 