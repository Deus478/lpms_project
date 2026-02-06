using System.Security.Claims;
using LegalCaseManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LegalCaseManagement.Infrastructure.Auth
{
    public class DevBypassAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public DevBypassAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<AuthOptions> options, LegalCaseDbContext db)
        {
            var authOptions = options.Value;

            if (!authOptions.DevBypassEnabled)
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            var userId = authOptions.DevBypassUserId;

            List<string> roleNames;
            try
            {
                roleNames = await db.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Include(ur => ur.Role)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();
            }
            catch
            {
                roleNames = new List<string>();
            }

            if (roleNames.Count == 0)
            {
                roleNames.Add("Admin");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new("userId", userId.ToString()),
                new(ClaimTypes.Name, $"DevUser{userId}")
            };

            foreach (var role in roleNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, authenticationType: "DevBypass");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }
    }
}
