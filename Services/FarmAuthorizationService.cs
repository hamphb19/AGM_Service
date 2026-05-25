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

        public async Task<long?> GetFarmIdForActionAsync(long actionId) =>
            await _context.FieldActions
                .Where(a => a.Id == actionId)
                .Select(a => (long?)a.Field.FarmId)
                .FirstOrDefaultAsync();
    }
}
