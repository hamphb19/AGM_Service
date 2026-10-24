using AGM_API.Database;
using AGM_API.Models.Farm.FarmMember;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Services
{
    /// <summary>
    /// Löscht ein Benutzerkonto (Apple Guideline 5.1.1(v)):
    /// - Farmen, in denen der Nutzer das einzige Mitglied ist, werden komplett gelöscht (inkl. Dateien).
    /// - In geteilten Farmen wird die Mitgliedschaft entfernt; bleibt kein Admin übrig, wird das
    ///   nächste Mitglied Admin und Eigentümer.
    /// - User und Person werden anonymisiert (sie bleiben nur als Referenz für Audit-Felder erhalten),
    ///   Aufgaben-Zuweisungen und Namen im Aktivitäts-Log werden entfernt.
    /// </summary>
    public class AccountDeletionService(AppDbContext context, FileStorageService storage)
    {
        public const string DeletedDisplayName = "Gelöschter Nutzer";

        public async Task<bool> DeleteAsync(long userId)
        {
            var user = await context.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            var deletedFarmIds = new List<long>();
            var deletedActionIds = new List<long>();

            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await context.Database.BeginTransactionAsync();
                deletedFarmIds.Clear();
                deletedActionIds.Clear();

                var person = user.Person;
                if (person != null)
                {
                    var adminRoleId = await context.MemberRoles
                        .Where(r => r.ShortName == "AD").Select(r => r.Id).FirstAsync();

                    var farmIds = await context.FarmMembers
                        .Where(m => m.person_Id == person.Id)
                        .Select(m => m.farm_Id).Distinct().ToListAsync();

                    foreach (var farmId in farmIds)
                    {
                        var members = await context.FarmMembers
                            .Include(m => m.Role)
                            .Where(m => m.farm_Id == farmId)
                            .ToListAsync();
                        var mine = members.Where(m => m.person_Id == person.Id).ToList();
                        var others = members.Where(m => m.person_Id != person.Id).ToList();

                        if (others.Count == 0)
                        {
                            deletedActionIds.AddRange(await context.FieldActions
                                .Where(a => a.Field.FarmId == farmId).Select(a => a.Id).ToListAsync());
                            var farm = await context.Farms.FindAsync(farmId);
                            if (farm != null) context.Farms.Remove(farm);
                            deletedFarmIds.Add(farmId);
                            continue;
                        }

                        context.FarmMembers.RemoveRange(mine);

                        var farmEntity = await context.Farms.Include(f => f.Owner).FirstAsync(f => f.Id == farmId);
                        var successor = others.FirstOrDefault(o => o.Role.ShortName == "AD")
                                        ?? others.OrderBy(o => o.person_Id).First();

                        if (!others.Any(o => o.Role.ShortName == "AD"))
                        {
                            // role_Id ist Teil des Schlüssels → Mitgliedschaft neu anlegen statt ändern.
                            context.FarmMembers.Remove(successor);
                            context.FarmMembers.Add(new FarmMember
                            {
                                farm_Id = farmId,
                                person_Id = successor.person_Id,
                                role_Id = adminRoleId,
                            });
                        }

                        if (farmEntity.Owner?.Id == person.Id)
                            farmEntity.Owner = await context.Persons.FindAsync(successor.person_Id);
                    }

                    var assignedTasks = await context.Tasks.Where(t => t.AssignedTo != null && t.AssignedTo.Id == person.Id).ToListAsync();
                    foreach (var t in assignedTasks) t.AssignedTo = null;

                    person.FirstName = null;
                    person.Name = DeletedDisplayName;
                }

                await context.ActivityLogs
                    .Where(a => a.UserId == userId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.UserId, (long?)null)
                        .SetProperty(a => a.UserDisplayName, DeletedDisplayName));

                user.Username = $"deleted-{user.Id}";
                user.Email = null;
                user.KeycloakSubject = null;
                user.UserCode = null;
                user.PasswordHash = null;
                user.IsAdmin = false;

                await context.SaveChangesAsync();
                await tx.CommitAsync();
            });

            foreach (var farmId in deletedFarmIds) storage.DeleteFolder($"farms/{farmId}");
            foreach (var actionId in deletedActionIds) storage.DeleteFolder($"fieldactions/{actionId}");
            return true;
        }
    }
}
