namespace AGM_API.Services.SeedData
{
    /// <summary>Root model for Services/SeedData/SeedData.json (reference/seed data).</summary>
    public class SeedData
    {
        public List<SeedNamedItem> FarmTypes { get; set; } = new();
        public List<SeedNamedItem> MemberRoles { get; set; } = new();
        public List<SeedNamedItem> CropTypes { get; set; } = new();
        public List<SeedCrop> Crops { get; set; } = new();
        public List<SeedNamedItem> FieldActionTypes { get; set; } = new();
        public List<SeedNamedItem> MachineTypes { get; set; } = new();
        public List<SeedNamedItem> MachineBrands { get; set; } = new();
        public List<SeedNamedItem> StallTypes { get; set; } = new();
        public List<SeedNamedItem> AnimalTypes { get; set; } = new();
        public List<SeedMachineModel> MachineModels { get; set; } = new();
    }

    public class SeedNamedItem
    {
        public string? Key { get; set; }
        public string Name { get; set; } = null!;
        public string ShortName { get; set; } = null!;
    }

    public class SeedCrop : SeedNamedItem
    {
        public string CropTypeShortName { get; set; } = null!;
    }

    public class SeedMachineModel
    {
        public string? Key { get; set; }
        public string Name { get; set; } = null!;
        public string BrandShortName { get; set; } = null!;
        public string TypeShortName { get; set; } = null!;
    }
}
