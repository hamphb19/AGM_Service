using System.Security.Claims;
using AGM_API.Services;

namespace AGM_API.Middleware
{
    /// <summary>
    /// After JWT validation, maps the Keycloak subject to a local user (provisioning
    /// on first sign-in) and injects <see cref="ClaimTypes.NameIdentifier"/> = local
    /// user id plus an "isAdmin" claim, so all existing controllers keep working
    /// unchanged (they read NameIdentifier as the local <c>User.Id</c>).
    /// </summary>
    public class KeycloakUserMiddleware
    {
        private readonly RequestDelegate _next;

        public KeycloakUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, KeycloakUserProvisioningService provisioning)
        {
            if (context.User.Identity?.IsAuthenticated == true
                && context.User.Identity is ClaimsIdentity identity)
            {
                var subject = context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(subject)
                    && identity.FindFirst(ClaimTypes.NameIdentifier) == null)
                {
                    var user = await provisioning.ResolveOrCreateAsync(context.User, subject);

                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    if (user.IsAdmin)
                        identity.AddClaim(new Claim("isAdmin", "true"));
                }
            }

            await _next(context);
        }
    }
}
