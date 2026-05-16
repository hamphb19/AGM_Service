using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Season
{
    [Table("SeasonField")]
    public class SeasonField
    {
        [Required]
        public long season_Id { get; set; }
        [ForeignKey(nameof(season_Id))]
        public Season Season { get; set; } = null!;
        [Required]
        public long field_Id { get; set; }
        [ForeignKey(nameof(field_Id))]
        public Field.Field Field { get; set; } = null!;
        public Crop.Crop? Crop { get; set; }

        public bool IsPlowed { get; set; } = false;
        public bool IsFertilized { get; set; } = false;
        public bool IsHarvested { get; set; } = false;

        [MaxLength(500)]
        public string? Notes { get; set; }

    }
}