using System.Security.Claims;

namespace AGM_API.Services
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Interne User-ID (gesetzt von KeycloakClaimsTransformation bzw. DevAuthHandler).</summary>
        public static long GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("No resolvable user id claim on the current principal.");
            return userId;
        }
    }
}
