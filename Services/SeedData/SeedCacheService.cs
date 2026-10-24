using AGM_API.Controllers.Records;
using AGM_API.Database;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Services.SeedData
{
    public static class SeedCacheService
    {
        private static Dictionary<string, MemberRoleReference> _memberroles = new();
        private static Dictionary<long, MemberRoleReference> _memberrolesByIdCache = new();
        private static Dictionary<string, CropReference> _crops = new();
        private static Dictionary<string, CropTypeReference> _croptypes = new();
        private static Dictionary<long, CropReference> _cropsByIdCache = new();

        private static bool _initialized = false;

        public static void Initialize(IServiceProvider services)
        {
            if (_initialized) return;
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                foreach (var kvp in SeedDataService.MemberRoleKeyToShortName)
                {
                    var role = context.MemberRoles.AsNoTracking().FirstOrDefault(x => x.ShortName == kvp.Value);
                    if (role != null)
                    {
                        var roleRef = new MemberRoleReference(role.Id, role.Name, role.ShortName);
                        _memberroles[kvp.Key] = roleRef;
                        _memberrolesByIdCache[role.Id] = roleRef;
                    }
                }

                foreach (var kvp in SeedDataService.CropTypeKeyToShortName)
                {
                    var type = context.CropTypes.AsNoTracking().FirstOrDefault(x => x.ShortName == kvp.Value);
                    if (type != null)
                        _croptypes[kvp.Key] = new CropTypeReference(type.Id, type.Name, type.ShortName);
                }

                foreach (var kvp in SeedDataService.CropKeyToShortName)
                {
                    var crop = context.Crops.AsNoTracking().FirstOrDefault(x => x.ShortName == kvp.Value);
                    if (crop != null)
                    {
                        var cropRef = new CropReference(crop.Id, crop.Name, crop.ShortName);
                        _crops[kvp.Key] = cropRef;
                        _cropsByIdCache[crop.Id] = cropRef;
                    }
                }

                _initialized = true;
            }
            catch
            {
                // log
            }
        }

        public static class MemberRoles
        {
            public static MemberRoleReference Administrator => _memberroles["AD"];
            public static MemberRoleReference Manager => _memberroles["MN"];
            public static MemberRoleReference Worker => _memberroles["WR"];
            public static MemberRoleReference Viewer => _memberroles["VW"];
        }

        public static class CropTypes
        {
            public static CropTypeReference Barley => _croptypes["Barley"];
            public static CropTypeReference Maize => _croptypes["Corn (Maize)"];
            public static CropTypeReference Wheat => _croptypes["Wheat"];
            public static CropTypeReference Soybean => _croptypes["Soybean"];
            public static CropTypeReference Pumpkin => _croptypes["Pumpkin"];
            public static CropTypeReference Oat => _croptypes["Oat"];
        }

        public static class Crops
        {
            public static CropReference Melissa => _crops["Melissa"];
            public static CropReference Ambitio => _crops["Ambitio"];
            public static CropReference Astronauto => _crops["Astronauto"];
            public static CropReference WinterWheat => _crops["Winter Wheat"];

            public static CropReference? GetById(long id) => _cropsByIdCache.GetValueOrDefault(id);
        }
    }
}
