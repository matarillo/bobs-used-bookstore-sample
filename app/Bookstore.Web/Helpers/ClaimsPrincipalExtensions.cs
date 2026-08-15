using System.Security.Claims;

namespace Bookstore.Web.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetSub(this ClaimsPrincipal claimsPrincipal)
        {
            // Returns null when the "sub" claim is missing, same as before; every caller already
            // treats an authenticated principal as one that carries it.
            return claimsPrincipal.FindFirst("sub")?.Value!;
        }
    }
}