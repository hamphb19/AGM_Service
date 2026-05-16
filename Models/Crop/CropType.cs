using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Crop
{
    [Table("CropType")]
    public class CropType
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;

        public ICollection<Crop> Crops { get; set; } = new List<Crop>();
    }
}
