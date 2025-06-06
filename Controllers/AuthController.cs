using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BookingSite.Models;
using Microsoft.AspNetCore.Authentication;

namespace BookingSite.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            var user = new ApplicationUser
            {
                Email = email,
                UserName = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }

            return Json(new { 
                success = false, 
                message = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { 
                success = false, 
                message = "Invalid email or password"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SetupAdmin()
        {
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                TempData["Error"] = "Admin credentials not found in environment variables";
                return RedirectToAction("Index", "Home");
            }

            // Check if the admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                // Create the Admin role
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                {
                    TempData["Error"] = "Failed to create Admin role";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Check if admin user exists
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Create the admin user
                adminUser = new ApplicationUser
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Failed to create admin user: " + string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Index", "Home");
                }

                // Add the user to the Admin role
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (!roleResult.Succeeded)
                {
                    TempData["Error"] = "Failed to add user to Admin role";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Success"] = "Admin user and role created successfully";
                return RedirectToAction("Index", "Home");
            }

            TempData["Success"] = "Admin user already exists";
            return RedirectToAction("Index", "Home");
        }
    }
} 