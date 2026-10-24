using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AGM_API.Services.Auth
{
    /// <summary>
    /// Local-development authentication: authenticates every request as a fixed
    /// dev user, so the API runs completely without Keycloak. Activated when
    /// configuration "Keycloak:Enabled" is false (e.g. appsettings.Development.json).
    /// The dev user is auto-provisioned like any Keycloak user via the claims
    /// transformation (subject "dev-local-user").
    /// </summary>
    public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "DevAuth";

        public DevAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new Claim("sub", "dev-local-user"),
                new Claim("email", "dev@local.test"),
                new Claim("preferred_username", "dev"),
                new Claim("given_name", "Dev"),
                new Claim("family_name", "User"),
                new Claim("isAdmin", "true"),
            };
            var identity = new ClaimsIdentity(claims, SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
