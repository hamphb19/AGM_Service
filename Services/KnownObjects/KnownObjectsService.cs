using AGM_API.Database;
using AGM_API.Models.Animal;
using AGM_API.Models.Farm.FarmMember;
using AGM_API.Models.Field;
using AGM_API.Models.Machine;
using System.Xml.Serialization;

namespace AGM_API.Services.KnownObjects
{
    public static class KnownObjectsService
    {
        public static Dictionary<string, string> MemberRoleKeyToShortName { get; private set; } = new();
        public static Dictionary<string, string> CropTypeKeyToShortName { get; private set; } = new();
        public static Dictionary<string, string> CropKeyToShortName { get; private set; } = new();

        public static void LoadFromKnownObjectsXml(AppDbContext context, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"KnownObjects not found: {filePath}");
            }

            var serializer = new XmlSerializer(typeof(KnownObjects));
            KnownObjects knownObjects;

            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    knownObjects = (KnownObjects)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading File {ex.Message}");
            }

            MemberRoleKeyToShortName = knownObjects.MemberRoles.ToDictionary(x => x.Key, x => x.ShortName);
            CropKeyToShortName = knownObjects.Crops.ToDictionary(x => x.Key, x => x.ShortName);
            CropTypeKeyToShortName = knownObjects.CropTypes.ToDictionary(x => x.Key, x => x.ShortName);

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                KnownObjectsMemberRoles(context, knownObjects.MemberRoles);
                KnownObjectsCropTypes(context, knownObjects.CropTypes);
                KnownObjectsCrops(context, knownObjects.Crops);
                KnownObjectsFieldActionTypes(context, knownObjects.FieldActionTypes);
                KnownObjectsStallTypes(context, knownObjects.StallTypes);
                KnownObjectsAnimalTypes(context, knownObjects.AnimalTypes);
                KnownObjectsMachineTypes(context, knownObjects.MachineTypes);
                KnownObjectsMachineBrands(context, knownObjects.MachineBrands);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading File {ex.Message}");
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        private static void KnownObjectsMemberRoles(AppDbContext context, List<MemberRoleXml> memberRoleXmls)
        {
            if (!memberRoleXmls.Any())
                return;

            var existingShortNames = context.MemberRoles
                  .Select(x => x.ShortName)
                  .ToHashSet();

            var newRoles = memberRoleXmls
                .Where(typeXml => !existingShortNames.Contains(typeXml.ShortName))
                .Select(typeXml => new MemberRole
                {
                    Name = typeXml.Name,
                    ShortName = typeXml.ShortName
                })
                .ToList();

            if (newRoles.Any())
            {
                context.MemberRoles.AddRange(newRoles);
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("✓ AnimalTypes already seeded");
            }
        }

        private static void KnownObjectsCropTypes(AppDbContext context, List<CropTypeXml> cropTypesXml)
        {
            if (!cropTypesXml.Any()) return;

            var existingShortNames = context.CropTypes.Select(ct => ct.ShortName).ToHashSet();

            var newTypes = cropTypesXml
                .Where(typeXml => !existingShortNames.Contains(typeXml.ShortName))
                .Select(typeXml => new Models.Crop.CropType
                {
                    Name = typeXml.Name,
                    ShortName = typeXml.ShortName
                })
                .ToList();

            if (newTypes.Any())
            {
                context.CropTypes.AddRange(newTypes);
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("✓ CropTypes already seeded");
            }
        }

        private static void KnownObjectsStallTypes(AppDbContext context, List<StallTypeXml> xmlItems)
        {
            if (!xmlItems.Any()) return;
            var existing = context.StallTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = xmlItems.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new StallType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Any()) { context.StallTypes.AddRange(newItems); context.SaveChanges(); }
            else Console.WriteLine("✓ StallTypes already seeded");
        }

        private static void KnownObjectsAnimalTypes(AppDbContext context, List<AnimalTypeXml> xmlItems)
        {
            if (!xmlItems.Any()) return;
            var existing = context.AnimalTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = xmlItems.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new AnimalType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Any()) { context.AnimalTypes.AddRange(newItems); context.SaveChanges(); }
            else Console.WriteLine("✓ AnimalTypes already seeded");
        }

        private static void KnownObjectsFieldActionTypes(AppDbContext context, List<FieldActionTypeXml> xmlItems)
        {
            if (!xmlItems.Any()) return;

            var existing = context.FieldActionTypes.Select(x => x.ShortName).ToHashSet();

            var newTypes = xmlItems
                .Where(x => !existing.Contains(x.ShortName))
                .Select(x => new FieldActionType { Name = x.Name, ShortName = x.ShortName })
                .ToList();

            if (newTypes.Any())
            {
                context.FieldActionTypes.AddRange(newTypes);
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("✓ FieldActionTypes already seeded");
            }
        }

        private static void KnownObjectsMachineTypes(AppDbContext context, List<MachineTypeXml> xmlItems)
        {
            if (!xmlItems.Any()) return;
            var existing = context.MachineTypes.Select(x => x.ShortName).ToHashSet();
            var newItems = xmlItems.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new MachineType { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Any()) { context.MachineTypes.AddRange(newItems); context.SaveChanges(); }
            else Console.WriteLine("✓ MachineTypes already seeded");
        }

        private static void KnownObjectsMachineBrands(AppDbContext context, List<MachineBrandXml> xmlItems)
        {
            if (!xmlItems.Any()) return;
            var existing = context.MachineBrands.Select(x => x.ShortName).ToHashSet();
            var newItems = xmlItems.Where(x => !existing.Contains(x.ShortName))
                .Select(x => new MachineBrand { Name = x.Name, ShortName = x.ShortName }).ToList();
            if (newItems.Any()) { context.MachineBrands.AddRange(newItems); context.SaveChanges(); }
            else Console.WriteLine("✓ MachineBrands already seeded");
        }

        private static void KnownObjectsCrops(AppDbContext context, List<CropXml> cropsXml)
        {
            if (!cropsXml.Any()) return;

            var cropTypes = context.CropTypes.ToList();
            var existingShortNames = context.Crops.Select(c => c.ShortName).ToHashSet();

            var newCrops = new List<Models.Crop.Crop>();

            foreach (var cropXml in cropsXml)
            {
                if (existingShortNames.Contains(cropXml.ShortName))
                    continue;

                var cropType = cropTypes.FirstOrDefault(ct => ct.ShortName == cropXml.CropTypeShortName);
                if (cropType == null)
                    continue;

                newCrops.Add(new Models.Crop.Crop
                {
                    Name = cropXml.Name,
                    ShortName = cropXml.ShortName,
                    CropTypeId = cropType.Id
                });
            }

            if (newCrops.Any())
            {
                context.Crops.AddRange(newCrops);
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("✓ Crops already seeded");
            }

        }
    }
}
