using System.Security.Claims;
using AGM_API.Database;
using AGM_API.Models;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Services
{
    /// <summary>
    /// Resolves the local <see cref="User"/> for an authenticated Keycloak principal,
    /// creating (auto-provisioning) it on first sign-in. Links to a pre-existing
    /// local account by e-mail when possible.
    /// </summary>
    public class KeycloakUserProvisioningService
    {
        private readonly AppDbContext _context;

        public KeycloakUserProvisioningService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> ResolveOrCreateAsync(ClaimsPrincipal principal, string subject)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubject == subject);
            if (existing != null)
                return existing;

            var email = principal.FindFirst("email")?.Value;
            var username = principal.FindFirst("preferred_username")?.Value
                           ?? email
                           ?? subject;
            var firstName = principal.FindFirst("given_name")?.Value;
            var lastName = principal.FindFirst("family_name")?.Value;

            // Link an existing local account (created before Keycloak) by e-mail.
            if (!string.IsNullOrEmpty(email))
            {
                var byEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.KeycloakSubject == null && u.Email == email);
                if (byEmail != null)
                {
                    byEmail.KeycloakSubject = subject;
                    await _context.SaveChangesAsync();
                    return byEmail;
                }
            }

            var user = new User
            {
                KeycloakSubject = subject,
                Email = email,
                Username = username,
                UserCode = await GenerateUniqueUserCodeAsync(),
            };
            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();

                _context.Persons.Add(new Models.Person.Person
                {
                    UserId = user.Id,
                    FirstName = firstName,
                    Name = string.IsNullOrWhiteSpace(lastName) ? username : lastName,
                });
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException)
            {
                // Concurrent first-request race: another request just created it.
                _context.ChangeTracker.Clear();
                return await _context.Users.FirstAsync(u => u.KeycloakSubject == subject);
            }
        }

        private async Task<string> GenerateUniqueUserCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string code;
            do
            {
                code = new string(Enumerable.Range(0, 6)
                    .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
            }
            while (await _context.Users.AnyAsync(u => u.UserCode == code));
            return code;
        }
    }
}
