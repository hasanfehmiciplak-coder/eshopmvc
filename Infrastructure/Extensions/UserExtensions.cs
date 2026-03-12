using System.Security.Claims;

namespace EShopMVC.Infrastructure.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}