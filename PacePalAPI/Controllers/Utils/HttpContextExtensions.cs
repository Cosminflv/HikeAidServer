using System.Security.Claims;

namespace PacePalAPI.Extensions
{
    public static class HttpContextExtensions
    {
        public static int? GetUserId(this HttpContext httpContext)
        {
            if (httpContext?.User == null) return null;

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return null;

            return int.TryParse(userIdClaim.Value, out int userId) ? userId : null;
        }
    }
}
