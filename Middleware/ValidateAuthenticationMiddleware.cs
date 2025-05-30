using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using BookingSite.Models;

namespace BookingSite.Middleware
{
    public class ValidateAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = userManager.GetUserId(context.User);
                if (userId != null)
                {
                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        // User no longer exists in database, sign them out
                        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                        context.Response.Redirect("/");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class ValidateAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidateAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidateAuthenticationMiddleware>();
        }
    }
} 