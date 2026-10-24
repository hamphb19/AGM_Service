using System.Security.Claims;

namespace AGM_API.Services
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Interne User-ID (gesetzt von KeycloakClaimsTransformation bzw. DevAuthHandler).</summary>
        public static long GetUserId(this ClaimsPrincipal user) =>
            long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
