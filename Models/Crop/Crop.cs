using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Crop
{
    [Table("Crop")]
    public class Crop
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;

        [Required]
        public long CropTypeId { get; set; }

        [ForeignKey(nameof(CropTypeId))]
        public CropType CropType { get; set; } = null!;

    }
}
