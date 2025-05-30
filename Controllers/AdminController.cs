using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingSite.Models;
using BookingSite.Data;

namespace BookingSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("/create-admin")]
        public async Task<IActionResult> CreateInitialAdmin()
        {
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                return BadRequest("Admin credentials not found in environment variables. Please set ADMIN_EMAIL and ADMIN_PASSWORD.");
            }

            // Check if any admin exists
            if (await _roleManager.RoleExistsAsync("Admin"))
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                if (adminUsers.Any())
                {
                    return BadRequest("Admin user already exists.");
                }
            }

            // Create Admin role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return Ok($"Admin user created successfully");
            }

            return BadRequest("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewData["CurrentUserId"] = _userManager.GetUserId(User);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["Success"] = $"User {user.Email} has been promoted to Admin.";
            }
            else
            {
                TempData["Error"] = "Failed to promote user to Admin.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            // Prevent self-deletion
            if (userId == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Dashboard));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Check if this is the original admin account
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            if (user.Email == adminEmail)
            {
                TempData["Error"] = "Cannot delete the original admin account.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Start a transaction to ensure all related data is deleted
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Here you would delete any related data
                // Example: await _context.Bookings.Where(b => b.UserId == userId).ExecuteDeleteAsync();

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to delete user.");
                }

                await transaction.CommitAsync();
                TempData["Success"] = $"User {user.Email} has been deleted.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Failed to delete user: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
} 