using AGM_API.Controllers.Records;
using AGM_API.Database;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace AGM_API.Services.KnownObjects
{
    public static class KnownCacheService
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

                // ---------- MemberRoles ---------- //
                foreach (var kvp in KnownObjectsService.MemberRoleKeyToShortName)
                {
                    var type = context.MemberRoles
                        .AsNoTracking()
                        .FirstOrDefault(x => x.ShortName == kvp.Value);

                    if (type != null)
                    {
                        var typeRef = new MemberRoleReference(type.Id, type.Name, type.ShortName);
                        _memberroles[kvp.Key] = typeRef;
                        _memberrolesByIdCache[type.Id] = typeRef;
                    }
                    // LOG would be good IDea

                }

                // ---------- CropTypes ---------- //
                foreach (var kvp in KnownObjectsService.CropTypeKeyToShortName)
                {
                    var type = context.CropTypes
                        .AsNoTracking()
                        .FirstOrDefault(x => x.ShortName == kvp.Value);

                    if (type != null)
                    {
                        var typeRef = new CropTypeReference(type.Id, type.Name, type.ShortName);
                    }
                }

                // ---------- Crops ---------- //
                foreach(var kvp in KnownObjectsService.CropKeyToShortName)
                {
                    var crop = context.Crops
                        .AsNoTracking()
                        .FirstOrDefault(x => x.ShortName == kvp.Value);

                    if(crop != null)
                    {
                        var cropRef = new CropReference(crop.Id, crop.Name, crop.ShortName);
                        _crops[kvp.Key] = cropRef;
                        _cropsByIdCache[crop.Id] = cropRef;
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                // log
            }
        }

        // KnownObjects
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
