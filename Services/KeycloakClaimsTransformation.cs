using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AGM_API.Services
{
    /// <summary>
    /// Runs as part of authentication: maps the Keycloak subject (sub) to a local
    /// user (provisioning on first sign-in) and injects
    /// <see cref="ClaimTypes.NameIdentifier"/> = local User.Id plus an "isAdmin"
    /// claim, so all existing controllers keep working unchanged.
    /// Unlike a pipeline middleware this reliably runs before authorization and
    /// controllers observe the resulting principal.
    /// </summary>
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        private readonly KeycloakUserProvisioningService _provisioning;

        public KeycloakClaimsTransformation(KeycloakUserProvisioningService provisioning)
        {
            _provisioning = provisioning;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
                return principal;

            // Already mapped for this principal (transform can run more than once).
            if (identity.FindFirst(ClaimTypes.NameIdentifier) != null)
                return principal;

            var subject = principal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(subject))
                return principal;

            var user = await _provisioning.ResolveOrCreateAsync(principal, subject);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            if (user.IsAdmin)
                identity.AddClaim(new Claim("isAdmin", "true"));

            return principal;
        }
    }
}
