using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Farm
{
    [Table("FarmType")]
    public class FarmType
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;
    }
}
