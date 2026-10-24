using AGM_API.Database;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Services
{
    public class FarmAuthorizationService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> IsMemberAsync(long farmId, long userId) =>
            await _context.FarmMembers
                .AnyAsync(m => m.farm_Id == farmId && m.Person.UserId == userId);

        public async Task<bool> IsAdminAsync(long farmId, long userId) =>
            await _context.FarmMembers
                .AnyAsync(m => m.farm_Id == farmId && m.Person.UserId == userId && m.Role.ShortName == "AD");

        public async Task<bool> CanWriteAsync(long farmId, long userId) =>
            await _context.FarmMembers
                .AnyAsync(m => m.farm_Id == farmId && m.Person.UserId == userId &&
                               (m.Role.ShortName == "AD" || m.Role.ShortName == "MN" || m.Role.ShortName == "WR"));

        public async Task<long?> GetFarmIdForFieldAsync(long fieldId) =>
            await _context.Fields
                .Where(f => f.Id == fieldId)
                .Select(f => (long?)f.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForMachineAsync(long machineId) =>
            await _context.Machines
                .Where(m => m.Id == machineId)
                .Select(m => (long?)m.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForMachineServiceAsync(long serviceId) =>
            await _context.MachineServices
                .Where(s => s.Id == serviceId)
                .Select(s => (long?)s.Machine.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForMachineReminderAsync(long reminderId) =>
            await _context.MachineReminders
                .Where(r => r.Id == reminderId)
                .Select(r => (long?)r.Machine.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForTaskAsync(long taskId) =>
            await _context.Tasks
                .Where(t => t.Id == taskId)
                .Select(t => (long?)t.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForSeasonAsync(long seasonId) =>
            await _context.Seasons
                .Where(s => s.Id == seasonId)
                .Select(s => (long?)s.Farm.Id)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForStallAsync(long stallId) =>
            await _context.Stalls
                .Where(s => s.Id == stallId)
                .Select(s => (long?)s.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForStallAnimalAsync(long stallAnimalId) =>
            await _context.StallAnimals
                .Where(a => a.Id == stallAnimalId)
                .Select(a => (long?)a.Stall.FarmId)
                .FirstOrDefaultAsync();

        public async Task<long?> GetFarmIdForStallEventAsync(long eventId) =>
            await _context.StallEvents
                .Where(e => e.Id == eventId)
                .Select(e => (long?)e.Stall.FarmId)
                .FirstOrDefaultAsync();

        /// <summary>Alle Farm-IDs, in denen der User Mitglied ist.</summary>
        public async Task<List<long>> GetFarmIdsForUserAsync(long userId) =>
            await _context.FarmMembers
                .Where(m => m.Person.UserId == userId)
                .Select(m => m.farm_Id)
                .Distinct()
                .ToListAsync();

        /// <summary>Behält nur Maschinen, die zur angegebenen Farm gehören.</summary>
        public async Task<List<long>> FilterMachineIdsForFarmAsync(IEnumerable<long> machineIds, long farmId)
        {
            var ids = machineIds.Distinct().ToList();
            return await _context.Machines
                .Where(m => ids.Contains(m.Id) && m.FarmId == farmId)
                .Select(m => m.Id)
                .ToListAsync();
        }

        /// <summary>Prüft, dass Saison, Felder und zugewiesene Person zur Farm gehören. Gibt eine Fehlermeldung oder null zurück.</summary>
        public async Task<string?> ValidateTaskReferencesAsync(long farmId, long? seasonId, IEnumerable<long>? fieldIds, long? personId)
        {
            if (seasonId.HasValue && !await _context.Seasons.AnyAsync(s => s.Id == seasonId.Value && s.Farm.Id == farmId))
                return "Season not found";

            if (fieldIds != null)
            {
                var ids = fieldIds.Distinct().ToList();
                var count = await _context.Fields.CountAsync(f => ids.Contains(f.Id) && f.FarmId == farmId);
                if (count != ids.Count) return "Field not found";
            }

            if (personId.HasValue && !await _context.FarmMembers.AnyAsync(m => m.farm_Id == farmId && m.person_Id == personId.Value))
                return "Person is not a member of this farm";

            return null;
        }

        public async Task<long?> GetFarmIdForActionAsync(long actionId) =>
            await _context.FieldActions
                .Where(a => a.Id == actionId)
                .Select(a => (long?)a.Field.FarmId)
                .FirstOrDefaultAsync();
    }
}
