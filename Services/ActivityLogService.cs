using AGM_API.Database;
using AGM_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AGM_API.Services
{
    public class ActivityLogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        public async System.Threading.Tasks.Task LogAsync(
            long farmId, string entityType, long? entityId, string action, string description)
        {
            var userId = GetCurrentUserId();
            var userName = userId.HasValue ? await GetDisplayNameAsync(userId.Value) : "System";

            context.ActivityLogs.Add(new ActivityLog
            {
                FarmId = farmId,
                UserId = userId,
                UserDisplayName = userName,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Description = description,
                Timestamp = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
        }

        private long? GetCurrentUserId()
        {
            var value = httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }

        private async System.Threading.Tasks.Task<string> GetDisplayNameAsync(long userId)
        {
            var person = await context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (person == null) return "Unbekannt";
            var first = person.FirstName?.Trim();
            var last = person.Name?.Trim();
            if (string.IsNullOrEmpty(first)) return last ?? "Unbekannt";
            return $"{first} {(last?.Length > 0 ? last[0] + "." : "")}".Trim();
        }
    }
}
