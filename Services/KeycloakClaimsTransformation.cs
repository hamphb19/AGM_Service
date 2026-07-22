using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AGM_API.Services
{
    /// <summary>
    /// Runs as part of authentication: maps the Keycloak subject (sub) to a local
    /// user (provisioning on first sign-in) and injects
    /// <see cref="ClaimTypes.NameIdentifier"/> = local User.Id plus an "isAdmin"
    /// claim, so all existing controllers keep working unchanged.
    /// </summary>
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        private readonly KeycloakUserProvisioningService _provisioning;
        private readonly ILogger<KeycloakClaimsTransformation> _logger;

        public KeycloakClaimsTransformation(
            KeycloakUserProvisioningService provisioning,
            ILogger<KeycloakClaimsTransformation> logger)
        {
            _provisioning = provisioning;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            {
                _logger.LogWarning("[KC-Transform] skipped: principal not authenticated");
                return principal;
            }

            // If a numeric local id is already present we are done.
            var existing = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(existing, out _))
                return principal;

            // Keycloak subject can arrive under several claim names depending on
            // inbound-claim mapping. Try them all.
            var subject = principal.FindFirst("sub")?.Value
                       ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                       ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? principal.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(subject))
            {
                _logger.LogWarning(
                    "[KC-Transform] no subject claim found. Present claim types: {Claims}",
                    string.Join(", ", principal.Claims.Select(c => c.Type)));
                return principal;
            }

            var user = await _provisioning.ResolveOrCreateAsync(principal, subject);

            // Replace any non-numeric NameIdentifier (e.g. the raw Keycloak GUID)
            // with the local numeric user id the controllers expect.
            foreach (var stale in identity.FindAll(ClaimTypes.NameIdentifier).ToList())
                identity.RemoveClaim(stale);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            if (user.IsAdmin)
                identity.AddClaim(new Claim("isAdmin", "true"));

            _logger.LogInformation("[KC-Transform] mapped sub {Sub} -> local user {UserId}", subject, user.Id);
            return principal;
        }
    }
}
