using System.Text.Json;
using AGM_API.Database;
using AGM_API.Models.Animal;
using AGM_API.Models.Farm.FarmMember;
using AGM_API.Models.Field;
using AGM_API.Models.Machine;

namespace AGM_API.Services.SeedData
{
    /// <summary>
    /// Seeds reference data from Services/SeedData/SeedData.json. Idempotent:
    /// only rows whose ShortName (or type+brand+name for models) does not yet
    /// exist are inserted, so it is safe to run on every startup.
    /// </summary>
    public static class SeedDataService
    {
        public static Dictionary<string, string> MemberRoleKeyToShortName { get; private set; } = new();
        public static Dictionary<string, string> CropTypeKeyToShortName { get; private set; } = new();
        public static Dictionary<string, string> CropKeyToShortName { get; private set; } = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        public static void LoadFromJson(AppDbContext context, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SeedData not found: {filePath}");

            SeedData data;
            try
            {
                using var stream = File.OpenRead(filePath);
                data = JsonSerializer.Deserialize<SeedData>(stream, JsonOptions)
                       ?? throw new Exception("SeedData.json deserialized to null");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading SeedData.json: {ex.Message}");
            }

            MemberRoleKeyToShortName = data.MemberRoles.Where(x => x.Key != null).ToDictionary(x => x.Key!, x => x.ShortName);
            CropKeyToShortName = data.Crops.Where(x => x.Key != null).ToDictionary(x => x.Key!, x => x.ShortName);
            CropTypeKeyToShortName = data.CropTypes.Where(x => x.Key != null).ToDictionary(x => x.Key!, x => x.ShortName);

            context.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                SeedFarmTypes(context, data.FarmTypes);
                SeedMemberRoles(context, data.MemberRoles);
                SeedCropTypes(context, data.CropTypes);
                SeedCrops(context, data.Crops);
                SeedFieldActionTypes(context, data.FieldActionTypes);
                SeedStallTypes(context, data.StallTypes);
                SeedAnimalTypes(context, data.AnimalTypes);
                SeedMachineTypes(context, data.MachineTypes);
                SeedMachineBrands(context, data.MachineBrands);
                SeedMachineModels(context, data.MachineModels);
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private static void SeedFarmTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.FarmTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new Models.Farm.FarmType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.FarmTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedMemberRoles(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.MemberRoles.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new MemberRole { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.MemberRoles.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedCropTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.CropTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new Models.Crop.CropType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.CropTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedFieldActionTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.FieldActionTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new FieldActionType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.FieldActionTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedStallTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.StallTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new StallType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.StallTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedAnimalTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.AnimalTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new AnimalType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.AnimalTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedMachineTypes(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.MachineTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new MachineType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.MachineTypes.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedMachineBrands(AppDbContext context, List<SeedNamedItem> items)
        {
            if (items.Count == 0) return;
            var existing = context.MachineBrands.Select(x => x.ShortName).ToHashSet();
            var newItems = items.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new MachineBrand { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Count > 0) { context.MachineBrands.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedMachineModels(AppDbContext context, List<SeedMachineModel> items)
        {
            if (items.Count == 0) return;
            var types = context.MachineTypes.ToList();
            var brands = context.MachineBrands.ToList();
            var existingKeys = context.MachineModels
                .Select(m => m.MachineTypeId + "_" + (m.MachineBrandId ?? 0) + "_" + m.Name)
                .ToHashSet();

            var newItems = new List<Models.Machine.MachineModel>();
            foreach (var item in items)
            {
                var type = types.FirstOrDefault(t => t.ShortName == item.TypeShortName);
                if (type == null) continue;
                var brand = brands.FirstOrDefault(b => b.ShortName == item.BrandShortName);
                var brandId = brand?.Id;
                var key = type.Id + "_" + (brandId ?? 0) + "_" + item.Name;
                if (existingKeys.Contains(key)) continue;
                newItems.Add(new Models.Machine.MachineModel
                {
                    Name = item.Name,
                    MachineTypeId = type.Id,
                    MachineBrandId = brandId,
                });
                existingKeys.Add(key);
            }
            if (newItems.Count > 0) { context.MachineModels.AddRange(newItems); context.SaveChanges(); }
        }

        private static void SeedCrops(AppDbContext context, List<SeedCrop> items)
        {
            if (items.Count == 0) return;
            var cropTypes = context.CropTypes.ToList();
            var existing = context.Crops.Select(c => c.ShortName).ToHashSet();

            var newCrops = new List<Models.Crop.Crop>();
            foreach (var item in items)
            {
                if (existing.Contains(item.ShortName)) continue;
                var cropType = cropTypes.FirstOrDefault(ct => ct.ShortName == item.CropTypeShortName);
                if (cropType == null) continue;
                newCrops.Add(new Models.Crop.Crop { Name = item.Name, ShortName = item.ShortName, CropTypeId = cropType.Id });
            }
            if (newCrops.Count > 0) { context.Crops.AddRange(newCrops); context.SaveChanges(); }
        }
    }
}
