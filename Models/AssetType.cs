using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;

namespace AGM_API.Models
{
    // This is more like Pig, Cow, John Deere but its abstract
    public abstract class AssetType : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;

    }
}
